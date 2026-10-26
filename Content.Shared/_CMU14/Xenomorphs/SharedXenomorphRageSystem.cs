using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphRageSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRageComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphRageComponent, GetMeleeAttackRateEvent>(OnAttackRate);
        SubscribeLocalEvent<XenomorphRageComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphRageComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSpeed(Entity<XenomorphRageComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var speed = 1 + ent.Comp.Rage * 0.028f;
        if (ent.Comp.ApprehendUntil > _timing.CurTime) speed *= 1.2f;
        args.ModifySpeed(speed, speed);
    }

    private void OnAttackRate(Entity<XenomorphRageComponent> ent, ref GetMeleeAttackRateEvent args)
        => args.Rate += ent.Comp.Rage * 0.28f;

    private void OnState(Entity<XenomorphRageComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnShutdown(Entity<XenomorphRageComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Rage = 0;
        ent.Comp.ApprehendUntil = TimeSpan.Zero;
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
