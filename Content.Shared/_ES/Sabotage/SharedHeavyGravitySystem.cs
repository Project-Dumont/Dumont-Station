// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Systems;
using Content.Shared.Standing;

namespace Content.Shared._ES.Sabotage;

public sealed partial class SharedHeavyGravitySystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _speed = default!;
    [Dependency] private StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeavyGravityComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HeavyGravityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HeavyGravityComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<HeavyGravityComponent, StandAttemptEvent>(OnStandAttempt);
    }

    private void OnStartup(Entity<HeavyGravityComponent> ent, ref ComponentStartup args)
    {
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnShutdown(Entity<HeavyGravityComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;
        ent.Comp.SpeedModifier = 1;
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnSpeed(Entity<HeavyGravityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.SpeedModifier, bypassImmunity: true);
    }

    private void OnStandAttempt(Entity<HeavyGravityComponent> ent, ref StandAttemptEvent args)
    {
        args.Cancel();
    }

    public void SetHeavyGravity(EntityUid uid, bool enabled)
    {
        if (enabled)
        {
            EnsureComp<HeavyGravityComponent>(uid);
            _standing.Down(uid, playSound: false, dropHeldItems: false, force: true);
        }
        else if (RemComp<HeavyGravityComponent>(uid))
        {
            _standing.Stand(uid);
        }
    }
}
