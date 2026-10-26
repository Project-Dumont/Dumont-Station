using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphReaperSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, KnockDownAttemptEvent>(OnKnockdown);
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, BeforeStatusEffectAddedEvent>(OnStatus);
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSpeed(Entity<XenomorphCarrionMantleComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active) args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    private void OnKnockdown(Entity<XenomorphCarrionMantleComponent> ent, ref KnockDownAttemptEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnStatus(Entity<XenomorphCarrionMantleComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (ent.Comp.Active && args.Effect.Id == "KnockedDown") args.Cancelled = true;
    }

    private void OnState(Entity<XenomorphCarrionMantleComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnShutdown(Entity<XenomorphCarrionMantleComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Active = false;
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
