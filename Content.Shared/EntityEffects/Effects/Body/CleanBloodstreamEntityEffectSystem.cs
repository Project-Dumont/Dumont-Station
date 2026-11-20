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
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.Body;

/// <summary>
/// Removes a given amount of chemicals from the bloodstream modified by scale.
/// Optionally ignores a given chemical.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class CleanBloodstreamEntityEffectSystem : EntityEffectSystem<BloodstreamComponent, CleanBloodstream>
{
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;

    protected override void Effect(Entity<BloodstreamComponent> entity, ref EntityEffectEvent<CleanBloodstream> args)
    {
        var scale = args.Scale * args.Effect.CleanseRate;

        // <Trauma>
        if (args.Effect.Excluded is { } excluded)
            _bloodstream.FlushChemicals((entity, entity), scale.Float(), excluded);
        else
            _bloodstream.FlushChemicals((entity, entity), scale.Float(), Array.Empty<ProtoId<ReagentPrototype>>());
        // </Trauma>
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class CleanBloodstream : EntityEffectBase<CleanBloodstream>
{
    /// <summary>
    ///     Amount of reagent we're cleaning out of our bloodstream.
    /// </summary>
    [DataField]
    public FixedPoint2 CleanseRate = 3.0f;

    /// <summary>
    ///     An optional chemical to ignore when doing removal.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype>[]? Excluded; // Trauma - made into an array

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-clean-bloodstream", ("chance", Probability));
}
