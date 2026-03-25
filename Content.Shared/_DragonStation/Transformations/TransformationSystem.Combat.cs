using Content.Goobstation.Common.Stunnable;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffect;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    private void InitializeCombat()
    {
        SubscribeLocalEvent<TransformationComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<TransformationComponent, GetUserMeleeDamageEvent>(OnGetUserMeleeDamage);
        SubscribeLocalEvent<TransformationComponent, BeforeOldStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<TransformationComponent, BeforeStunEvent>(OnBeforeStun);
        SubscribeLocalEvent<TransformationComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<TransformationComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<TransformationComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<TransformationComponent, FighterProgressionChangedEvent>(OnFighterProgressionChanged);
    }

    private void OnRefreshMovementSpeed(EntityUid uid, TransformationComponent component, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!component.Active)
            return;

        var bonuses = _fighterProgression.GetBonuses(uid);
        var speed = component.SpeedModifier * bonuses.TransformationSpeedMultiplier;
        args.ModifySpeed(speed, speed);
    }

    private void OnGetUserMeleeDamage(EntityUid uid, TransformationComponent component, ref GetUserMeleeDamageEvent args)
    {
        if (!component.Active)
            return;

        var bonuses = _fighterProgression.GetBonuses(uid);
        args.Damage *= component.MeleeDamageModifier * bonuses.TransformationMeleeMultiplier;
    }

    // Active transformations can provide their own defensive profile.
    private void OnDamageModify(EntityUid uid, TransformationComponent component, ref DamageModifyEvent args)
    {
        if (!component.Active)
            return;

        var bonuses = _fighterProgression.GetBonuses(uid);
        var resistances = new DamageModifierSet
        {
            Coefficients =
            {
                ["Blunt"] = component.BluntResistanceCoefficient * bonuses.TransformationResistanceCoefficientMultiplier,
                ["Slash"] = component.SlashResistanceCoefficient * bonuses.TransformationResistanceCoefficientMultiplier,
                ["Piercing"] = component.PiercingResistanceCoefficient * bonuses.TransformationResistanceCoefficientMultiplier,
                ["Heat"] = component.HeatResistanceCoefficient * bonuses.TransformationResistanceCoefficientMultiplier,
                ["Cold"] = component.ColdResistanceCoefficient * bonuses.TransformationResistanceCoefficientMultiplier,
            }
        };

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage,
            DamageSpecifier.PenetrateArmor(resistances, args.Damage.ArmorPenetration));
    }

    private void OnBeforeStatusEffect(EntityUid uid, TransformationComponent component, ref BeforeOldStatusEffectAddedEvent args)
    {
        if (!component.Active || !component.SuppressStunEffects)
            return;

        if (args.EffectKey is not ("KnockedDown" or "Stun"))
            return;

        args.Cancelled = true;
    }

    private void OnBeforeStun(EntityUid uid, TransformationComponent component, ref BeforeStunEvent args)
    {
        if (!component.Active || !component.SuppressStunEffects)
            return;

        args.Cancelled = true;
    }

    // Critical condition or death immediately knocks the user out of the form.
    private void OnMobStateChanged(EntityUid uid, TransformationComponent component, MobStateChangedEvent args)
    {
        if (!component.Active
            || args.NewMobState is not (MobState.Critical or MobState.Dead))
            return;

        DisableTransformation(uid, component);
    }

    // Falling asleep shuts the form off immediately.
    private void OnSleep(EntityUid uid, TransformationComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.Active
            || !args.FellAsleep)
            return;

        DisableTransformation(uid, component);
    }

    private void OnFighterProgressionChanged(EntityUid uid, TransformationComponent component, FighterProgressionChangedEvent args)
    {
        RefreshActionAvailability(uid, component);
    }
}
