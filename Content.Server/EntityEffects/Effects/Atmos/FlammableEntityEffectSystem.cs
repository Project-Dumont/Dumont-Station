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

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.Atmos;

namespace Content.Server.EntityEffects.Effects.Atmos;

/// <summary>
/// Adds a number of FireStacks modified by scale to this entity.
/// The amount of FireStacks added is modified by scale.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class FlammableEntityEffectSystem : EntityEffectSystem<FlammableComponent, Flammable>
{
    [Dependency] private FlammableSystem _flammable = default!;

    protected override void Effect(Entity<FlammableComponent> entity, ref EntityEffectEvent<Flammable> args)
    {
        // The multiplier is determined by if the entity is already on fire, and if the multiplier for existing FireStacks has a value.
        // If both of these are true, we use the MultiplierOnExisting value, otherwise we use the standard Multiplier.
        var multiplier = entity.Comp.FireStacks == 0f || args.Effect.MultiplierOnExisting == null ? args.Effect.Multiplier : args.Effect.MultiplierOnExisting.Value;

        _flammable.AdjustFireStacks(entity, args.Scale * multiplier, entity.Comp,
            fireProtectionPenetration: args.Effect.FireProtectionPenetration); // Goob
    }
}
