// Dumont start
using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
// Dumont end
using Content.Shared.Store;

namespace Content.Goobstation.Shared.ManifestListings;

[ByRefEvent]
public record struct PrependObjectivesSummaryTextEvent(string Text = "");

[ByRefEvent]
public readonly record struct ListingPurchasedEvent(EntityUid User, EntityUid Store, ListingDataWithCostModifiers Data, IReadOnlyDictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Cost);
