// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.EntityConditions;
using Content.Shared.Mind.Components;

namespace Content.Trauma.Shared.EntityConditions;

/// <summary>
/// Checks that the target mob had mind at some point
/// </summary>
public sealed partial class HadMindCondition : EntityConditionBase<HadMindCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
        => string.Empty;
}

public sealed partial class HadMindConditionSystem : EntityConditionSystem<MindContainerComponent, HadMindCondition>
{
    protected override void Condition(Entity<MindContainerComponent> ent, ref EntityConditionEvent<HadMindCondition> args)
    {
        // Dumont start
        args.Result = ent.Comp.Mind != null || ent.Comp.LastMindStored != null;
        // Dumont end
    }
}
