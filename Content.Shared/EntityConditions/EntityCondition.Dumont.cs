// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.EntityConditions.Conditions;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions;

// Dumont start
public abstract partial class EntityCondition
{
    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (this is ReagentCondition reagent && args is EntityEffectReagentArgs { Source: { } solution })
        {
            var quantity = solution.GetTotalPrototypeQuantity(reagent.Reagent);
            return reagent.Inverted != (quantity >= reagent.Min && quantity <= reagent.Max);
        }

        return args.EntityManager.System<SharedEntityConditionsSystem>()
            .TryCondition(args.TargetEntity, this, (args as EntityEffectUserArgs)?.User);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
        => EntityConditionGuidebookText(prototype);
}
// Dumont end
