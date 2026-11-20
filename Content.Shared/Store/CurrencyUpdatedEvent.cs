using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.Store;

// Dumont start
public sealed class CurrencyUpdatedEvent : EntityEventArgs
{
    public readonly Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Currency;

    public CurrencyUpdatedEvent(Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> currency)
    {
        Currency = currency;
    }

    public CurrencyUpdatedEvent(Dictionary<string, FixedPoint2> currency)
    {
        Currency = currency.ToDictionary(e => (ProtoId<CurrencyPrototype>) e.Key, e => e.Value);
    }
}


// Dumont end
