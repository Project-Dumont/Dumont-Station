using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphResinWalkerSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinWalkerComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphResinWalkerComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnSpeed(Entity<XenomorphResinWalkerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active && ent.Comp.OnWeeds)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    private void OnState(Entity<XenomorphResinWalkerComponent> ent, ref AfterAutoHandleStateEvent args) =>
        _movement.RefreshMovementSpeedModifiers(ent);
}
