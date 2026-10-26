using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using Robust.Shared.Player;
using System.Linq;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphRoyalSystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedEyeSystem _eye = default!;
    [Dependency] private Content.Server.Chat.Managers.IChatManager _chat = default!;
    private readonly Dictionary<EntityUid, (string Mode, EntityUid? Target)> _messages = new();
    private TimeSpan _nextHiveUpdate;

    private void InitializeHive()
    {
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenHiveActionEvent>(OnHive);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenMessageActionEvent>(OnMessage);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenUiMessage>(OnHiveMessage);
        SubscribeLocalEvent<XenomorphRoyalComponent, MobStateChangedEvent>(OnQueenState);
        SubscribeLocalEvent<XenomorphRoyalComponent, ComponentShutdown>(OnQueenRemoved);
    }
    private void OpenHive(Entity<XenomorphRoyalComponent> ent, string mode = "hive", EntityUid? target = null)
    {
        if (!Ready(ent)) return;
        _messages[ent] = (mode, target);
        RefreshHive(ent, mode);
        _ui.TryOpenUi(ent.Owner, XenomorphQueenUiKey.Key, ent.Owner);
    }
    private void RefreshHive(Entity<XenomorphRoyalComponent> ent, string mode)
    {
        var members = new List<XenomorphQueenMember>();
        var query = EntityQueryEnumerator<XenomorphLineageComponent, XenomorphComponent>();
        while (query.MoveNext(out var uid, out _, out var xeno))
            if (uid != ent.Owner && Sister(ent, uid)) members.Add(new XenomorphQueenMember(GetNetEntity(uid), Name(uid), xeno.Caste.Id, ent.Comp.Leaders.Contains(uid), EntityManager.System<XenomorphHiveRanksSystem>().Tier(xeno.Caste)));
        _ui.SetUiState(ent.Owner, XenomorphQueenUiKey.Key,
            new XenomorphQueenUiState(members, mode, Attached(ent), GetNetEntity(ent.Comp.Watched), _pheromones.GetValueOrDefault(ent.Owner, "")));
    }
    private void OnHive(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenHiveActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        OpenHive(ent);
        args.Handled = true;
    }
    private void OnMessage(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenMessageActionEvent args)
    {
        if (args.Handled || !Ready(ent) || args.Mode is not ("word" or "order" or "pheromones")) return;
        OpenHive(ent, args.Mode, args.Mode == "order" ? ent.Comp.Watched : null);
        args.Handled = true;
    }
    private void OnQueenState(Entity<XenomorphRoyalComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive) StopWatching(ent);
        if (TryComp<Content.Shared.Sound.Components.SpamEmitSoundComponent>(ent, out var breathing))
            EntityManager.System<Content.Shared.Sound.SharedEmitSoundSystem>().SetEnabled((ent.Owner, breathing), args.NewMobState == MobState.Alive);
        if (args.NewMobState == MobState.Dead)
            _audio.PlayPvs(new Robust.Shared.Audio.SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg", Robust.Shared.Audio.AudioParams.Default.WithVolume(-6)), ent);
    }
    private void OnQueenRemoved(Entity<XenomorphRoyalComponent> ent, ref ComponentShutdown args)
    {
        StopWatching(ent);
        _messages.Remove(ent);
    }
    private void StopWatching(Entity<XenomorphRoyalComponent> ent)
    {
        if (ent.Comp.Watched == null) return;
        if (TryComp<XenomorphWatchComponent>(ent, out var watch)) EntityManager.System<XenomorphWatchSystem>().Stop((ent.Owner, watch));
        ent.Comp.Watched = null;
        _eye.SetTarget(ent, null);
    }
    private bool CanCommandAt(Entity<XenomorphRoyalComponent> ent, EntityCoordinates target)
    {
        var origin = ent.Comp.Watched is { } watched && Sister(ent, watched) ? watched : ent.Owner;
        return _transform.GetMapId(target) == Transform(ent).MapID
            && _interaction.InRangeUnobstructed(origin, target, 7);
    }
    private void OnHiveMessage(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenUiMessage args)
    {
        if (args.Actor != ent.Owner || !Ready(ent) || !Equals(args.UiKey, XenomorphQueenUiKey.Key)) return;
        var target = GetEntity(args.Target);
        if (args.Operation is "frenzy" or "warding" or "recovery" or "stopphero")
        { _messages[ent] = ("pheromones", null); ChoosePheromones(ent, args.Operation); RefreshHive(ent, "pheromones"); return; }
        switch (args.Operation)
        {
            case "return": StopWatching(ent); break;
            case "watch":
                if (target is not { } watched || !Sister(ent, watched)) return;
                if (TryComp<XenomorphWatchComponent>(ent, out var watch)) EntityManager.System<XenomorphWatchSystem>().Watch((ent.Owner, watch), watched);
                break;
            case "leader":
                if (target is not { } leader || leader == ent.Owner || !Sister(ent, leader)) return;
                ent.Comp.Leaders.RemoveAll(uid => !Exists(uid) || _mobs.IsDead(uid));
                if (!ent.Comp.Leaders.Remove(leader) && ent.Comp.Leaders.Count < 3) ent.Comp.Leaders.Add(leader);
                break;
            case "send": SendQueenMessage(ent, args.Text); break;
        }
        RefreshHive(ent, _messages.TryGetValue(ent, out var mode) ? mode.Mode : "hive");
    }
    public bool SendQueenMessage(Entity<XenomorphRoyalComponent> ent, string text)
    {
        text = text.Trim();
        if (!Ready(ent) || text.Length == 0 || text.Length > 500 || !_messages.TryGetValue(ent, out var message)) return false;
        var cost = message.Mode == "order" ? 100 : 50;
        if (message.Mode is not ("word" or "order")) return false;
        if (message.Mode == "order" && (message.Target is not { } target || !Exists(target)
            || !Sister(ent, target))) return false;
        if (!_plasma.ChangePlasmaAmount(ent, -cost)) return false;
        if (message.Mode == "word") EntityManager.System<XenomorphHiveCaptureSystem>().PlayHiveCommandSound();
        var query = EntityQueryEnumerator<ActorComponent>();
        while (query.MoveNext(out var uid, out var actor))
        {
            var receives = uid == ent.Owner || (message.Mode == "word" && HasComp<XenomorphLineageComponent>(uid) && HasComp<XenomorphComponent>(uid))
                || (message.Mode == "order" && message.Target == uid);
            if (!receives) continue;
            if (message.Mode == "word")
                RaiseNetworkEvent(new XenomorphQueenAnnouncementEvent(text), actor.PlayerSession);
            var prefix = Loc.GetString(message.Mode == "word" ? "xenomorph-queen-word-prefix" : "xenomorph-queen-psychic-prefix");
            var wrapped = $"{prefix} {FormattedMessage.EscapeText(text)}";
            _chat.ChatMessageToOne(ChatChannel.Local, text, wrapped, ent, false, actor.PlayerSession.Channel, Color.FromHex("#b56dd6"), recordReplay: true, hidePopup: message.Mode == "word");
        }
        _messages.Remove(ent);
        _ui.CloseUi(ent.Owner, XenomorphQueenUiKey.Key, ent.Owner);
        return true;
    }
    private void UpdateHive()
    {
        UpdatePheromones();
        var query = EntityQueryEnumerator<XenomorphRoyalComponent>();
        while (query.MoveNext(out var uid, out var queen))
        {
            if (queen.Watched is { } watched && !Sister(uid, watched)) StopWatching((uid, queen));
        }
        if (_nextHiveUpdate > _timing.CurTime) return;
        _nextHiveUpdate = _timing.CurTime + TimeSpan.FromSeconds(2);
        var queens = EntityQueryEnumerator<XenomorphRoyalComponent>();
        while (queens.MoveNext(out var uid, out var queen))
        {
            UpdateOvipositorActions(uid);
            if (_messages.TryGetValue(uid, out var mode)) RefreshHive((uid, queen), mode.Mode);
        }
    }
}
