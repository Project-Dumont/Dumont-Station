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

using JetBrains.Annotations;
// Dumont end

namespace Content.Shared.Store;

/// <summary>
/// Used to define a complicated condition that requires C#
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class ListingCondition
{
    /// <summary>
    /// Determines whether or not a certain entity can purchase a listing.
    /// </summary>
    /// <returns>Whether or not the listing can be purchased</returns>
    public abstract bool Condition(ListingConditionArgs args);
}

/// <param name="Buyer">Either the account owner, user, or an inanimate object (e.g., surplus bundle)</param>
/// <param name="Listing">The listing itself</param>
/// <param name="EntityManager">An entitymanager for sane coding</param>
public readonly record struct ListingConditionArgs(EntityUid Buyer, EntityUid? StoreEntity, ListingData Listing, IEntityManager EntityManager);
