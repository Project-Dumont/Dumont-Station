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

namespace Content.Shared.EntityEffects.Effects.StatusEffects;

/// <summary>
/// Entity effect that specifically deals with new status effects.
/// </summary>
/// <typeparam name="T">The entity effect type, typically for status effects which need systems to pass arguments</typeparam>
public abstract partial class BaseStatusEntityEffect<T> : EntityEffectBase<T> where T : BaseStatusEntityEffect<T>
{
    /// <summary>
    /// How long the modifier applies (in seconds).
    /// Is scaled by reagent amount if used with an EntityEffectReagentArgs.
    /// </summary>
    [DataField]
    public TimeSpan? Time = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Should this effect add the status effect, remove time from it, or set its cooldown?
    /// </summary>
    [DataField]
    public StatusEffectMetabolismType Type = StatusEffectMetabolismType.Update;

    /// <summary>
    /// Delay before the effect starts. If another effect is added with a shorter delay, it takes precedence.
    /// </summary>
    [DataField]
    public TimeSpan StatusDelay;
}
