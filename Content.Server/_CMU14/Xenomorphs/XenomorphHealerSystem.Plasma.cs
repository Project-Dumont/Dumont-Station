using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphHealerSystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedActionsSystem _actions = default!;

    private bool CanTransfer(EntityUid user, EntityUid target)
        => user != target && _mobs.IsAlive(user) && _blocker.CanConsciouslyPerformAction(user)
            && _blocker.CanInteract(user, target) && HasComp<XenomorphComponent>(target) && !_mobs.IsDead(target)
            && _interaction.InRangeUnobstructed(user, target, 1.5f)
            && (!TryComp<XenomorphRestComponent>(user, out var rest) || !rest.Resting)
            && (!TryComp<XenomorphOvipositorComponent>(target, out var ovi) || !ovi.Attached)
            && TryComp<PlasmaVesselComponent>(target, out var vessel) && vessel.Plasma < vessel.MaxPlasma
            && _plasma.HasPlasma(user, 100);

    private void SetTransferActive(EntityUid user, bool active)
    {
        foreach (var action in _actions.GetActions(user))
            if (_actions.GetEvent(action) is XenomorphHealerPlasmaActionEvent)
                _actions.SetToggled(action.Owner, active);
    }

    private void OnPlasma(Entity<XenomorphHealerComponent> ent, ref XenomorphHealerPlasmaActionEvent args)
    {
        if (args.Handled) return;
        if (!CanTransfer(ent, args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-healer-plasma-invalid"), ent, ent);
            return;
        }
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(1.5),
            new XenomorphHealerPlasmaDoAfterEvent(), ent.Owner, args.Target)
        {
            BreakOnMove = true, DistanceThreshold = 1.5f, NeedHand = false,
            BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent
        });
        if (!args.Handled) return;
        SetTransferActive(ent, true);
        Spawn("DumontXenomorphPlasmaTransferEffect", new EntityCoordinates(args.Target, Vector2.Zero));
    }

    private void OnPlasmaTransferred(Entity<XenomorphHealerComponent> ent, ref XenomorphHealerPlasmaDoAfterEvent args)
    {
        if (args.Handled) return;
        if (args.Cancelled || args.Target is not { } target || !CanTransfer(ent, target))
        {
            SetTransferActive(ent, false);
            return;
        }
        _plasma.ChangePlasmaAmount(ent, -100);
        _plasma.ChangePlasmaAmount(target, 100);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_drool1.ogg"), ent);
        _popup.PopupEntity(Loc.GetString("xenomorph-healer-plasma-received"), target, target);
        args.Handled = true;
        args.Repeat = CanTransfer(ent, target);
        SetTransferActive(ent, args.Repeat);
        if (args.Repeat) Spawn("DumontXenomorphPlasmaTransferEffect", new EntityCoordinates(target, Vector2.Zero));
    }
}
