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

using Content.Shared.EntityConditions;

using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Shared.EntityEffects;

/// <summary>
/// A prototype for entity effects which can be reused via <see cref="NestedEffect"/>.
/// </summary>
[Prototype]
public sealed partial class EntityEffectPrototype: IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// The effects of this prototype.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;

    /// <summary>
    /// Conditions checked for this effect, regardless of the <see cref="NestedEffect"/> using it.
    /// Currently not included in the guidebook text!
    /// </summary>
    [DataField]
    public EntityEffectCondition[]? Conditions;

    /// <summary>
    /// An override for the effect guidebook text, has "chance" passed from 0 to 1.
    /// By default one is generated from each effect.
    /// </summary>
    [DataField]
    public LocId? GuidebookText;
}
