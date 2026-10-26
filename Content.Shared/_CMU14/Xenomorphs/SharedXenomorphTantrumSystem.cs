using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphTantrumSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphTantrumComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphTantrumComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphTantrumComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnSpeed(Entity<XenomorphTantrumComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Running) args.ModifySpeed(ent.Comp.Speed, ent.Comp.Speed);
    }
    private void OnState(Entity<XenomorphTantrumComponent> ent, ref AfterAutoHandleStateEvent args) => _movement.RefreshMovementSpeedModifiers(ent);
    private void OnShutdown(Entity<XenomorphTantrumComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Speed = 1;
        if (!TerminatingOrDeleted(ent)) _movement.RefreshMovementSpeedModifiers(ent);
    }
}
