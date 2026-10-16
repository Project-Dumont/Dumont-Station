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

using Content.Shared.Humanoid;
using Content.Shared.Store;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Server.Store.Conditions;

/// <summary>
/// Allows a store entry to be filtered out based on the user's species.
/// Supports both blacklists and whitelists.
/// </summary>
public sealed partial class BuyerSpeciesCondition : ListingCondition
{
    /// <summary>
    /// A whitelist of species that can purchase this listing.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>>? Whitelist;

    /// <summary>
    /// A blacklist of species that cannot purchase this listing.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;

        if (!ent.TryGetComponent<MindComponent>(args.Buyer, out var mind))
            return true; // needed to obtain body entityuid to check for humanoid appearance

        if (!ent.TryGetComponent<HumanoidProfileComponent>(mind.OwnedEntity, out var humanoid))
            return true; // inanimate or non-humanoid entities should be handled elsewhere, main example being surplus crates

        if (Blacklist != null)
        {
            if (Blacklist.Contains(humanoid.Species))
                return false;
        }

        if (Whitelist != null)
        {
            if (!Whitelist.Contains(humanoid.Species))
                return false;
        }

        return true;
    }
}
