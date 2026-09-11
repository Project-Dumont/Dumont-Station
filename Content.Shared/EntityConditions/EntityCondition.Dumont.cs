// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions;

// Dumont start
public abstract partial class EntityCondition
{
    public override bool Condition(EntityEffectBaseArgs args)
        => args.EntityManager.System<SharedEntityConditionsSystem>()
            .TryCondition(args.TargetEntity, this, (args as EntityEffectUserArgs)?.User);

    public override string GuidebookExplanation(IPrototypeManager prototype)
        => EntityConditionGuidebookText(prototype);
}
// Dumont end
