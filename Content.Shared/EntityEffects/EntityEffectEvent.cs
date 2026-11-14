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

namespace Content.Shared.EntityEffects;

/// <summary>
/// An Event carrying an entity effect.
/// </summary>
/// <param name="Effect">The Effect</param>
/// <param name="Scale">A strength scalar for the effect, defaults to 1 and typically only goes under for incomplete reactions.</param>
/// <param name="User">The entity causing the effect.</param>
[ByRefEvent, Access(typeof(SharedEntityEffectsSystem))]
public readonly record struct EntityEffectEvent<T>(T Effect, float Scale, EntityUid? User, bool Predicted) where T : EntityEffectBase<T> // Trauma - added predicted
{
    /// <summary>
    /// The Condition being raised in this event
    /// </summary>
    public readonly T Effect = Effect;

    /// <summary>
    /// The Scale modifier of this Effect.
    /// </summary>
    public readonly float Scale = Scale;

    /// <summary>
    /// The entity that caused this effect.
    /// Used for admin logs and prediction purposes.
    /// </summary>
    public readonly EntityUid? User = User;

    /// <summary>
    /// Trauma - Whether this effect can be predicted or not.
    /// If only the server runs it, it can never be predicted.
    /// </summary>
    public readonly bool Predicted = Predicted;
}
