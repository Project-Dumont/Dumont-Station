using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.GameTicking.Rules;
using Content.Shared.Store;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class ListingCostRuleSystem : GameRuleSystem<ListingCostRuleComponent>
{
    public void ApplyModifiers(IEnumerable<ListingData> listings)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            foreach (var listing in listings)
            {
                if (!comp.Modifiers.TryGetValue(listing.ID, out var modifier))
                    continue;

                listing.Cost = listing.Cost.ToDictionary(x => x.Key,
                    x => FixedPoint2.New(MathF.Round(x.Value.Float() * modifier)));
            }
        }
    }
}
