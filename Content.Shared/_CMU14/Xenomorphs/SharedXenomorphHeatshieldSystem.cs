using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphHeatshieldSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHeatshieldComponent, DamageModifyEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, GetMeleeDamageEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, GetMeleeAttackRateEvent>(OnAttackRate);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphHeatshieldComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnDamage(Entity<XenomorphHeatshieldComponent> ent, ref DamageModifyEvent args)
    {
        if (!args.Damage.DamageDict.TryGetValue("Heat", out var heat) || heat <= 0)
            return;
        args.Damage = new DamageSpecifier(args.Damage);
        args.Damage.DamageDict["Heat"] = heat * ent.Comp.FireDamageMultiplier;
    }

    private void OnMelee(Entity<XenomorphHeatshieldComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (HasComp<OnFireComponent>(ent))
            args.Damage += ent.Comp.BurningMeleeDamage;
    }

    private void OnAttackRate(Entity<XenomorphHeatshieldComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (ent.Comp.Thermoregulating)
            args.Multipliers *= ent.Comp.AttackRateMultiplier;
    }

    private void OnSpeed(Entity<XenomorphHeatshieldComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Thermoregulating)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    private void OnState(Entity<XenomorphHeatshieldComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);
}
