using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;

namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    private void InitializeStamina()
    {
        SubscribeLocalEvent<TransformationComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
    }

    // Drain stamina while transformed and force the form off before the user stamcrits.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TransformationComponent, StaminaComponent>();
        while (query.MoveNext(out var uid, out var transformation, out var staminaComp))
        {
            if (!transformation.Active)
                continue;

            var bonuses = _fighterProgression.GetBonuses(uid);
            _stamina.TakeStaminaDamage(uid, transformation.StaminaDrainRate * bonuses.TransformationStaminaDrainMultiplier * frameTime, staminaComp, source: uid, visual: false);

            if (staminaComp.StaminaDamage >= staminaComp.CritThreshold - transformation.LowStaminaThreshold)
                DisableTransformation(uid, transformation);
        }
    }

    // Transformations can ignore outside stamina damage while still paying their own upkeep cost.
    private void OnBeforeStaminaDamage(EntityUid uid, TransformationComponent component, ref BeforeStaminaDamageEvent args)
    {
        if (!component.Active || !component.IgnoreExternalStaminaDamage)
            return;

        if (args.Value <= 0)
            return;

        if (args.Source == uid)
            return;

        args.Value = 0f;
        args.Cancelled = true;
    }
}
