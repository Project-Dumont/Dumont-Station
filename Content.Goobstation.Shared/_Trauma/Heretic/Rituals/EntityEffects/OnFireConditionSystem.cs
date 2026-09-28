// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Atmos.Components;
using Content.Shared.EntityConditions;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed class OnFireConditionSystem : EntityConditionSystem<FlammableComponent, OnFireCondition>
{
    protected override void Condition(Entity<FlammableComponent> entity, ref EntityConditionEvent<OnFireCondition> args)
    {
        args.Result = entity.Comp.OnFire;
    }
}

public sealed partial class OnFireCondition : EntityConditionBase<OnFireCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-on-fire", ("invert", Inverted));
    }
}
