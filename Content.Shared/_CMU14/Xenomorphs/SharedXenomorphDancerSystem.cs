using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphDancerSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphDancerComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphDancerComponent, AttemptMobCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphDancerComponent, AttemptMobTargetCollideEvent>(OnTargetCollide);
        SubscribeLocalEvent<XenomorphDancerComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphDancerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSpeed(Entity<XenomorphDancerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.Dodging) return;
        var speed = ent.Comp.InCrowd ? 1.6f : 1.35f;
        args.ModifySpeed(speed, speed);
    }

    private void OnCollide(Entity<XenomorphDancerComponent> ent, ref AttemptMobCollideEvent args)
    {
        if (ent.Comp.Dodging) args.Cancelled = true;
    }

    private void OnTargetCollide(Entity<XenomorphDancerComponent> ent, ref AttemptMobTargetCollideEvent args)
    {
        if (ent.Comp.Dodging) args.Cancelled = true;
    }

    private void OnState(Entity<XenomorphDancerComponent> ent, ref AfterAutoHandleStateEvent args) => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnShutdown(Entity<XenomorphDancerComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Dodging = false;
        if (!TerminatingOrDeleted(ent)) _movement.RefreshMovementSpeedModifiers(ent);
    }
}
