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
using Content.Shared.Whitelist;
// Dumont end

namespace Content.Server.Store.Conditions;

/// <summary>
/// Filters out an entry based on the components or tags on the store itself.
/// </summary>
public sealed partial class StoreWhitelistCondition : ListingCondition
{
    /// <summary>
    /// A whitelist of tags or components.
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// A blacklist of tags or components.
    /// </summary>
    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        if (args.StoreEntity == null)
            return false;

        var ent = args.EntityManager;
        var whitelistSystem = ent.System<EntityWhitelistSystem>();

        if (whitelistSystem.IsWhitelistFail(Whitelist, args.StoreEntity.Value) ||
            whitelistSystem.IsWhitelistPass(Blacklist, args.StoreEntity.Value))
            return false;

        return true;
    }
}
