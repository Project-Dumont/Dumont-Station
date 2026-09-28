// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.Heretic.Effects;

/// <summary>
/// Spills an amount of the target's blood onto the floor in a puddle.
/// </summary>
public sealed partial class SpillBlood : EntityEffectBase<SpillBlood>
{
    [DataField(required: true)]
    public FixedPoint2 Amount;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Spills target blood.";
}

public sealed partial class SpillBloodEffectSystem : EntityEffectSystem<BloodstreamComponent, SpillBlood>
{
    [Dependency] private SharedPuddleSystem _puddle = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    protected override void Effect(Entity<BloodstreamComponent> ent, ref EntityEffectEvent<SpillBlood> args)
    {
        if (!_solution.ResolveSolution(ent.Owner, ent.Comp.BloodSolutionName, ref ent.Comp.BloodSolution, out var bloodSolution))
            return;

        var amount = args.Effect.Amount;
        _puddle.TrySpillAt(ent, bloodSolution.SplitSolution(amount), out _);
    }
}
