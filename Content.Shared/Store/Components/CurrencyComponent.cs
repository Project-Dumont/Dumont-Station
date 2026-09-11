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

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Shared.Store.Components;

/// <summary>
/// Identifies a component that can be inserted into a store
/// to increase its balance.
/// </summary>
/// <remarks>
/// Note that if this entity is a stack of items, then this is meant to represent the value per stack item, not
/// the whole stack. This also means that in general, the actual value should not be modified from the initial
/// prototype value because otherwise stack merging/splitting may modify the total value.
/// </remarks>
[RegisterComponent]
public sealed partial class CurrencyComponent : Component
{
    /// <summary>
    /// The value of the currency.
    /// The string is the currency type that will be added.
    /// The FixedPoint2 is the value of each individual currency entity.
    /// </summary>
    /// <remarks>
    /// Note that if this entity is a stack of items, then this is meant to represent the value per stack item, not
    /// the whole stack. This also means that in general, the actual value should not be modified from the initial
    /// prototype value
    /// because otherwise stack merging/splitting may modify the total value.
    /// </remarks>
    [DataField(required: true)]
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Price = new();
}
