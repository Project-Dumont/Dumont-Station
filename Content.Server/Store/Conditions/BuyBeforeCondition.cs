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

using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Server.Store.Conditions;

public sealed partial class BuyBeforeCondition : ListingCondition
{
    /// <summary>
    ///     Required listing(s) needed to purchase before this listing is available
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ListingPrototype>>? Whitelist;

    /// <summary>
    ///     Listing(s) that if bought, block this purchase, if any.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ListingPrototype>>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        if (!args.EntityManager.TryGetComponent<StoreComponent>(args.StoreEntity, out var storeComp))
            return false;

        var allListings = storeComp.FullListingsCatalog;

        var purchasesFound = false;

        if (Blacklist != null)
        {
            foreach (var blacklistListing in Blacklist)
            {
                foreach (var listing in allListings)
                {
                    if (listing.ID == blacklistListing.Id && listing.PurchaseAmount > 0)
                        return false;
                }
            }
        }

        if (Whitelist != null)
        {
            foreach (var requiredListing in Whitelist)
            {
                foreach (var listing in allListings)
                {
                    if (listing.ID == requiredListing.Id)
                    {
                        purchasesFound = listing.PurchaseAmount > 0;
                        // <Trauma>
                        if (purchasesFound != WhitelistRequireAll)
                            return purchasesFound;
                        // </Trauma>
                        break;
                    }
                }
            }
            return purchasesFound;
        }

        return true;
    }
}
