using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Orion.Mobs.Critical;

public sealed partial class SharedCritStateSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CritStateMovementComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CritStateMovementComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovement);
    }

    private void OnMobStateChanged(Entity<CritStateMovementComponent> ent, ref MobStateChangedEvent args)
    {
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMovement(Entity<CritStateMovementComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!_mobState.IsSoftCritical(ent))
            return;

        args.ModifySpeed(ent.Comp.SoftCritWalkModifier, ent.Comp.SoftCritSprintModifier);
    }
}
