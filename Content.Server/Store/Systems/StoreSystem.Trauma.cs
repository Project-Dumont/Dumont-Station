// SPDX-License-Identifier: AGPL-3.0-or-later

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

using Content.Server.Polymorph.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Polymorph;
using Content.Shared.Store;
using Content.Shared.Store.Components;
// Dumont end

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private SharedGameTicker _ticker = default!;

    [SubscribeLocalEvent]
    private void OnPolymorphed(Entity<StoreComponent> ent, ref PolymorphedEvent args)
    {
        if (args.IsRevert)
            return;

        _polymorph.CopyPolymorphComponent<StoreComponent>(ent, args.NewEntity);
    }

    private void OnPurchase(ListingData listing)
    {
        if (!ProtoMan.Resolve<ListingPrototype>(listing.ID, out var prototype))
            return;

        // updating restocktime
        var now = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan);
        if (prototype.ResetRestockOnPurchase)
        {
            var restockDuration = prototype.RestockTime;
            listing.RestockTime = now + restockDuration;
        }
        if (listing.ResetRestockOnPurchase)
        {
            var restockDuration = listing.RestockAfterPurchase ?? listing.RestockTime;
            listing.RestockTime = now + restockDuration;
        }
    }
}
