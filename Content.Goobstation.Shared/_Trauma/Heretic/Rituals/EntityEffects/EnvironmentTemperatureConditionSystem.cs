// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Atmos;
using Content.Shared.EntityConditions;
using Content.Trauma.Shared.Heretic.Components;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed class EnvironmentTemperatureConditionSystem : EntityConditionSystem<TemperatureTrackerComponent, EnvironmentTemperatureCondition>
{
    protected override void Condition(Entity<TemperatureTrackerComponent> entity,
        ref EntityConditionEvent<EnvironmentTemperatureCondition> args)
    {
        args.Result = entity.Comp.Temperature < args.Condition.Threshold;
    }
}

public sealed partial class EnvironmentTemperatureCondition : EntityConditionBase<EnvironmentTemperatureCondition>
{
    [DataField]
    public float Threshold = Atmospherics.T0C;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("entity-condition-guidebook-environment-temperature",
            ("invert", Inverted),
            ("threshold", Threshold));
    }
}
