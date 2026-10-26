using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphBulwarkSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBulwarkComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphBulwarkComponent, GetMeleeDamageEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphBulwarkComponent, KnockDownAttemptEvent>(OnKnockdown);
        SubscribeLocalEvent<XenomorphBulwarkComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnSpeed(Entity<XenomorphBulwarkComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Encased)
            args.ModifySpeed(ent.Comp.EncasedSpeedMultiplier, ent.Comp.EncasedSpeedMultiplier);
    }

    private void OnDamage(Entity<XenomorphBulwarkComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.Encased)
            args.Damage += ent.Comp.EncasedMeleePenalty;
    }

    private void OnKnockdown(Entity<XenomorphBulwarkComponent> ent, ref KnockDownAttemptEvent args)
    {
        if (ent.Comp.Encased)
            args.Cancelled = true;
    }

    private void OnState(Entity<XenomorphBulwarkComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);
}
