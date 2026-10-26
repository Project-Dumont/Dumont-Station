using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphHivelordSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinWalkerComponent, XenomorphResinWalkerActionEvent>(OnWalker);
        SubscribeLocalEvent<XenomorphResinWalkerComponent, XenomorphHivelordPlasmaActionEvent>(OnTransfer);
        SubscribeLocalEvent<XenomorphResinWalkerComponent, XenomorphHivelordPlasmaDoAfterEvent>(OnTransferred);
    }

    private void SetActive(Entity<XenomorphResinWalkerComponent> ent, bool active)
    {
        ent.Comp.Active = active;
        ent.Comp.NextDrain = _timing.CurTime + TimeSpan.FromSeconds(0.5);
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphResinWalkerActionEvent)
                _actions.SetToggled(action.Owner, active);
    }

    private void OnWalker(Entity<XenomorphResinWalkerComponent> ent, ref XenomorphResinWalkerActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent))
            return;
        if (!ent.Comp.Active && !_plasma.ChangePlasmaAmount(ent, -50))
            return;
        SetActive(ent, !ent.Comp.Active);
        args.Handled = true;
    }

    private bool CanTransfer(EntityUid user, EntityUid target)
    {
        return user != target && _mobs.IsAlive(user) && HasComp<XenomorphComponent>(target)
            && _blocker.CanInteract(user, target)
            && _interaction.InRangeUnobstructed(user, target, 7)
            && (!TryComp<XenomorphOvipositorComponent>(target, out var ovi) || !ovi.Attached)
            && TryComp<PlasmaVesselComponent>(target, out var vessel) && vessel.Plasma < vessel.MaxPlasma
            && _plasma.HasPlasma(user, 200);
    }

    private void OnTransfer(Entity<XenomorphResinWalkerComponent> ent, ref XenomorphHivelordPlasmaActionEvent args)
    {
        if (args.Handled)
            return;
        if (!CanTransfer(ent, args.Target))
        {
            var message = !HasComp<XenomorphComponent>(args.Target) || args.Target == ent.Owner
                ? "xenomorph-plasma-transfer-invalid"
                : !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 7)
                    ? "xenomorph-plasma-transfer-range"
                    : !_plasma.HasPlasma(ent, 200)
                        ? "xenomorph-plasma-transfer-empty"
                        : TryComp<XenomorphOvipositorComponent>(args.Target, out var ovi) && ovi.Attached
                            ? "xenomorph-plasma-transfer-ovipositor"
                            : "xenomorph-plasma-transfer-full";
            _popup.PopupEntity(Loc.GetString(message), ent, ent);
            return;
        }
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(0.5),
            new XenomorphHivelordPlasmaDoAfterEvent(), ent.Owner, args.Target)
        {
            BreakOnMove = true,
            DistanceThreshold = 7,
            NeedHand = false,
        });
        if (args.Handled)
            Spawn("DumontXenomorphPlasmaTransferEffect", new EntityCoordinates(args.Target, Vector2.Zero));
    }

    private void OnTransferred(Entity<XenomorphResinWalkerComponent> ent, ref XenomorphHivelordPlasmaDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target || !CanTransfer(ent, target))
            return;
        _plasma.ChangePlasmaAmount(ent, -200);
        _plasma.ChangePlasmaAmount(target, 200);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_drool1.ogg"), ent);
        args.Handled = true;
        args.Repeat = CanTransfer(ent, target);
        if (args.Repeat)
            Spawn("DumontXenomorphPlasmaTransferEffect", new EntityCoordinates(target, Vector2.Zero));
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphResinWalkerComponent, XenomorphComponent>();
        while (query.MoveNext(out var uid, out var walker, out var xeno))
        {
            if (walker.OnWeeds != xeno.OnWeed)
            {
                walker.OnWeeds = xeno.OnWeed;
                Dirty(uid, walker);
                _movement.RefreshMovementSpeedModifiers(uid);
            }
            if (!walker.Active)
                continue;
            if (!_mobs.IsAlive(uid))
            {
                SetActive((uid, walker), false);
                continue;
            }
            if (_timing.CurTime < walker.NextDrain)
                continue;
            walker.NextDrain = _timing.CurTime + TimeSpan.FromSeconds(0.5);
            if (!_plasma.ChangePlasmaAmount(uid, -15))
                SetActive((uid, walker), false);
        }
    }
}
