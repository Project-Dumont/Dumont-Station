using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphHedgehogSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHedgehogComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphHedgehogComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphHedgehogComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSpeed(Entity<XenomorphHedgehogComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.ShedUntil > _timing.CurTime) args.ModifySpeed(1.45f, 1.45f);
    }

    private void OnState(Entity<XenomorphHedgehogComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnShutdown(Entity<XenomorphHedgehogComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.ShedUntil = TimeSpan.Zero;
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
