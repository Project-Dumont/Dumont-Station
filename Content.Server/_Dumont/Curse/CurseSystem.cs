using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared._Dumont.CCVar;
using Content.Shared._Dumont.Curse;
using Content.Shared._Dumont.EntityEffects;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Dumont.Curse;

public sealed class CurseSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CurseCavityBombSystem _cavityBomb = default!;

    private bool _enabled;
    private float _doubleChance;
    private readonly HashSet<Guid> _cursedUsers = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, DumontCVars.CurseEnabled, value => _enabled = value, true);
        Subs.CVar(_cfg, DumontCVars.CurseUserIds, ParseUserIds, true);
        Subs.CVar(_cfg, DumontCVars.CurseDoubleChance, value => _doubleChance = value, true);

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<ToggleCurse>>(OnToggleCurse);
    }

    private void ParseUserIds(string value)
    {
        _cursedUsers.Clear();

        foreach (var entry in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Guid.TryParse(entry, out var id))
                _cursedUsers.Add(id);
            else
                Log.Warning($"Invalid user id in {DumontCVars.CurseUserIds.Name}: {entry}");
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!_enabled || !_cursedUsers.Contains(args.Player.UserId.UserId))
            return;

        TryCurse(args.Mob);
    }

    private void OnToggleCurse(ref ExecuteEntityEffectEvent<ToggleCurse> args)
    {
        var uid = args.Args.TargetEntity;

        if (args.Args is EntityEffectReagentArgs { Source: { } source, Reagent: { } reagent })
            source.RemoveReagent(reagent.ID, source.GetTotalPrototypeQuantity(reagent.ID));

        if (HasComp<CursedComponent>(uid))
            RemoveCurse(uid);
        else
            TryCurse(uid);
    }

    public void TryCurse(EntityUid uid)
    {
        if (HasComp<CursedComponent>(uid) || PickCurse() is not { } curse)
            return;

        _adminLog.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(uid):player} rolled curse {curse.ID}");

        if (curse.Components.Count == 0)
            return;

        var curses = new List<ProtoId<MasonCursePrototype>> { curse.ID };
        if (_random.Prob(_doubleChance) && PickCurse(curses) is { } extra)
        {
            curses.Add(extra.ID);
            _adminLog.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(uid):player} rolled extra curse {extra.ID}");
        }

        foreach (var id in curses)
        {
            EntityManager.AddComponents(uid, _proto.Index(id).Components);
        }

        var cursed = EnsureComp<CursedComponent>(uid);
        cursed.Curses = curses;
        Dirty(uid, cursed);

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var popup = Loc.GetString("curse-subtle-popup");
        var message = Loc.GetString(curses.Count > 1 ? "curse-subtle-message-double" : "curse-subtle-message");
        _popup.PopupEntity(popup, uid, actor.PlayerSession, PopupType.Large);
        _chat.ChatMessageToOne(ChatChannel.Local, message, $"{popup} \"{message}\"", EntityUid.Invalid, false, actor.PlayerSession.Channel);
    }

    public void RemoveCurse(EntityUid uid)
    {
        if (!TryComp<CursedComponent>(uid, out var cursed))
            return;

        foreach (var id in cursed.Curses)
        {
            if (_proto.TryIndex(id, out var curse))
                EntityManager.RemoveComponents(uid, curse.Components);
        }

        RemComp<CursedComponent>(uid);
        _cavityBomb.DisarmBombs(uid);
        _popup.PopupEntity(Loc.GetString("curse-removed-popup"), uid, uid, PopupType.Medium);
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(uid):player} had their curses {string.Join(", ", cursed.Curses)} removed");
    }

    /// <summary>
    /// Picks a weighted curse. With <paramref name="exclude"/>, only real curses not in it can be picked.
    /// </summary>
    private MasonCursePrototype? PickCurse(ICollection<ProtoId<MasonCursePrototype>>? exclude = null)
    {
        var curses = _proto.EnumeratePrototypes<MasonCursePrototype>()
            .Where(c => exclude == null || c.Components.Count > 0 && !exclude.Contains(c.ID))
            .ToList();

        var total = curses.Sum(c => c.Weight);
        if (total <= 0)
            return null;

        var roll = _random.NextFloat(total);
        foreach (var curse in curses)
        {
            roll -= curse.Weight;
            if (roll <= 0)
                return curse;
        }

        return curses[^1];
    }
}
