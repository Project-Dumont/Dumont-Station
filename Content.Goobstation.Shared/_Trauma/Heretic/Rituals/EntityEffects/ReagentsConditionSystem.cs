// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityConditions;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Fluids.Components;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed partial class ReagentsEntityConditionSystem : EntityConditionSystem<PuddleComponent, ReagentsCondition>
{
    [Dependency] private SharedSolutionContainerSystem _sol = default!;


    protected override void Condition(Entity<PuddleComponent> entity,
        ref EntityConditionEvent<ReagentsCondition> args)
    {
        if (!_sol.TryGetSolution(entity.Owner, entity.Comp.SolutionName, out _, out var sol))
            return;

        // Dumont start
        var reagents = new string[args.Condition.Reagents.Length];
        for (var i = 0; i < reagents.Length; i++)
            reagents[i] = args.Condition.Reagents[i].Id;

        var quant = sol.GetTotalPrototypeQuantity(reagents);
        // Dumont end

        args.Result = quant > args.Condition.Min && quant < args.Condition.Max;
    }
}

public sealed partial class ReagentsCondition : EntityConditionBase<ReagentsCondition>
{
    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField]
    public ProtoId<ReagentPrototype>[] Reagents =
    [
        "Blood",
        "AmmoniaBlood",
        "InsectBlood",
        "CopperBlood",
        "ZombieBlood",
        "AlienBlood",
        "BlackBlood",
        "BloodChangeling",
    ];

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        if (!prototype.Resolve(Reagents[0], out var reagentProto))
            return string.Empty;

        return Loc.GetString("entity-condition-guidebook-reagent-threshold",
            ("reagent", reagentProto.LocalizedName),
            ("max", Max == FixedPoint2.MaxValue ? int.MaxValue : Max.Float()),
            ("min", Min.Float()));
    }
}
