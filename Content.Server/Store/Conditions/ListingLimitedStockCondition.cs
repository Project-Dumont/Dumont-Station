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

using Content.Shared.Store;
// Dumont end

namespace Content.Server.Store.Conditions;

/// <summary>
/// Only allows a listing to be purchased a certain amount of times.
/// </summary>
public sealed partial class ListingLimitedStockCondition : ListingCondition
{
    /// <summary>
    /// The amount of times this listing can be purchased.
    /// </summary>
    [DataField("stock", required: true)]
    public int Stock;

    public override bool Condition(ListingConditionArgs args)
    {
        return args.Listing.PurchaseAmount < Stock;
    }
}
