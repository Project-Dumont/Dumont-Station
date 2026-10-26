using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Network;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphCrestSystem : EntitySystem
{
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphCrestComponent, XenomorphCrestActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphCrestComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphCrestComponent, KnockDownAttemptEvent>(OnKnockdown);
        SubscribeLocalEvent<XenomorphCrestComponent, MobStateChangedEvent>(OnMobState);
    }

    private void OnAction(Entity<XenomorphCrestComponent> ent, ref XenomorphCrestActionEvent args)
    {
        if (_net.IsClient || args.Handled || !_mobState.IsAlive(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting
            || TryComp<XenomorphFortifyComponent>(ent, out var fortify) && fortify.Fortified)
            return;

        ent.Comp.Action = args.Action.Owner;
        SetLowered(ent, !ent.Comp.Lowered);
        args.Handled = true;
    }

    private void OnSpeed(Entity<XenomorphCrestComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Lowered)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    private void OnKnockdown(Entity<XenomorphCrestComponent> ent, ref KnockDownAttemptEvent args)
    {
        if (ent.Comp.Lowered)
            args.Cancelled = true;
    }

    private void OnMobState(Entity<XenomorphCrestComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_net.IsClient && ent.Comp.Lowered && args.NewMobState != MobState.Alive)
            SetLowered(ent, false);
    }

    private void SetLowered(Entity<XenomorphCrestComponent> ent, bool lowered)
    {
        ent.Comp.Lowered = lowered;
        Dirty(ent);
        _armor.RefreshAlert(ent);
        _actions.SetToggled(ent.Comp.Action, lowered);
        _appearance.SetData(ent, XenomorphVisuals.Crest, lowered);
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
