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

namespace Content.Shared.Store;

/// <summary>
///     Prototype used to define different types of currency for generic stores.
///     Mainly used for antags, such as traitors, nukies, and revenants
///     This is separate to the cargo ordering system.
/// </summary>
[Prototype]
[DataDefinition]
public sealed partial class CurrencyPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The Loc string used for displaying the currency in the store ui.
    /// doesn't necessarily refer to the full name of the currency, only
    /// that which is displayed to the user.
    /// </summary>
    [DataField(required: true)] // Trauma - required
    public LocId DisplayName; // Trauma - use LocId

    /// <summary>
    /// The physical entity of the currency
    /// </summary>
    [DataField]
    public Dictionary<FixedPoint2, EntProtoId>? Cash { get; private set; }

    /// <summary>
    /// Whether or not this currency can be withdrawn from a shop by a player. Requires a valid entityId.
    /// </summary>
    [DataField]
    public bool CanWithdraw { get; private set; } = true;
}
