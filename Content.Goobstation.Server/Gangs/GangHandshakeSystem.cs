using Content.Goobstation.Shared.Gangs;
using Content.Goobstation.Server.Gangs.GameTicking.Rules;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Server.Antag;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Gangs;

public sealed class GangHandshakeSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly GangRuleSystem _gangRuleSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangLeaderComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PendingGangHandshakeComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbsPending);
    }

    private void OnGetVerbs(EntityUid uid, GangLeaderComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanAccess
            || !args.CanInteract
            || args.Target == args.User)
            return;

        // check the target
        if (!TryComp<MobStateComponent>(args.Target, out var targetMobState)
            || !_mobState.IsAlive(args.Target, targetMobState))
            return;

        // check the leader
        if (!TryComp<MobStateComponent>(uid, out var leaderMobState)
            || !_mobState.IsAlive(uid, leaderMobState))
            return;

        if (TryComp<GangMemberComponent>(args.Target, out var targetMember) && targetMember.GangId == comp.GangId)
            return;

        if (HasComp<GangMemberComponent>(args.Target)
            || HasComp<PendingGangHandshakeComponent>(args.Target))
            return;

        InnateVerb handshakeVerb = new()
        {
            Act = () => OfferHandshake(args.User, args.Target),
            Text = Loc.GetString("gang-handshake-verb", ("target", args.Target)),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Clothing/Head/Hats/Gang/tophat.rsi"), "icon"),
            Priority = 1
        };
        args.Verbs.Add(handshakeVerb);
    }

    private void OfferHandshake(EntityUid user, EntityUid target)
    {
        var pending = AddComp<PendingGangHandshakeComponent>(target);
        pending.Offerer = user;
        pending.ExpiryTime = _timing.CurTime + TimeSpan.FromSeconds(15);

        _popup.PopupEntity(Loc.GetString("gang-handshake-offer", ("user", user)), target, target);
        _popup.PopupEntity(Loc.GetString("gang-handshake-offer-self", ("target", target)), user, user);
    }

    private void OnGetVerbsPending(EntityUid uid, PendingGangHandshakeComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract ||
            _mobState.IsIncapacitated(uid) ||
            args.Target != comp.Offerer)
        {
            return;
        }

        InnateVerb handshakeVerb = new()
        {
            Act = () => AcceptHandshake(uid, comp.Offerer),
            Text = Loc.GetString("gang-handshake-accept-verb", ("user", comp.Offerer)),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Clothing/Head/Hats/Gang/tophat.rsi"), "icon"),
            Priority = 1
        };
        args.Verbs.Add(handshakeVerb);
    }

    private void AcceptHandshake(EntityUid target, EntityUid offerer)
    {
        if (!TryComp<GangLeaderComponent>(offerer, out var leaderComp))
        {
            _popup.PopupEntity(Loc.GetString("gang-handshake-invalid"), target, target);
            RemComp<PendingGangHandshakeComponent>(target);
            return;
        }

        var memberComp = EnsureComp<GangMemberComponent>(target);
        memberComp.GangId = leaderComp.GangId;
        leaderComp.Members.Add(target);

        ForceHat(target, leaderComp.GangId);

        GangRuleComponent? gangRule = null;
        var query = EntityQueryEnumerator<GangRuleComponent>();
        while (query.MoveNext(out var ruleUid, out var ruleComp))
        {
            gangRule = ruleComp;
            break;
        }

        if (gangRule != null)
        {
            var briefing = Loc.GetString(gangRule.GangMemberGreeting);
            _antag.SendBriefing(target, briefing, Color.Yellow, gangRule.MemberBriefingSound);
        }

        _popup.PopupEntity(Loc.GetString("gang-handshake-accepted-self", ("target", target)), offerer, offerer);
        RemComp<PendingGangHandshakeComponent>(target);
    }

    private void ForceHat(EntityUid memberUid, EntityUid gangId)
    {
        var hatProto = _gangRuleSystem.GetHatProto(gangId);
        if (hatProto == null)
            return;

        var hat = Spawn(hatProto, Transform(memberUid).Coordinates);

        if (_inventory.TryGetSlotEntity(memberUid, "neck", out var existingHat))
            _inventory.TryUnequip(memberUid, "neck");


        _inventory.TryEquip(memberUid, hat, "neck");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PendingGangHandshakeComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ExpiryTime > curTime)
                continue;

            RemCompDeferred<PendingGangHandshakeComponent>(uid);
            _popup.PopupEntity(Loc.GetString("gang-handshake-expired"), uid, uid);
        }
    }
}
