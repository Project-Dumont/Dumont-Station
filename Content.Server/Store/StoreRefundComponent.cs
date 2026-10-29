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

using Content.Server.Store.Systems;
// Dumont end

namespace Content.Server.Store.Components;

// TODO: Refund on a per-item/action level.
//   Requires a refund button next to each purchase (disabled/invis by default)
//   Interactions with ActionUpgrades would need to be modified to reset all upgrade progress and return the original action purchase to the store.

/// <summary>
///     Keeps track of entities bought from stores for refunds, especially useful if entities get deleted before they can be refunded.
/// </summary>
[RegisterComponent, Access(typeof(StoreSystem))]
public sealed partial class StoreRefundComponent : Component
{
    /// <summary>
    ///     The store this entity was bought from
    /// </summary>
    [DataField]
    public EntityUid? StoreEntity;

    /// <summary>
    ///     The time this entity was bought
    /// </summary>
    [DataField]
    public TimeSpan? BoughtTime;

    /// <summary>
    ///     How long until this entity disables refund purchase?
    /// </summary>
    [DataField]
    public TimeSpan DisableTime = TimeSpan.FromSeconds(300);
}
