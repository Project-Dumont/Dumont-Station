using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphSpitterSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphSpitterComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphSpitterComponent, AfterAutoHandleStateEvent>(OnState);
    }
    private void OnSpeed(Entity<XenomorphSpitterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Charging)
            args.ModifySpeed(1.4f, 1.4f);
    }
    private void OnState(Entity<XenomorphSpitterComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);
}
