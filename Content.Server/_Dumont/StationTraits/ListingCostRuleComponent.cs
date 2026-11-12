using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.StationTraits;

[RegisterComponent]
public sealed partial class ListingCostRuleComponent : Component
{
    [DataField(required: true)]
    public Dictionary<ProtoId<ListingPrototype>, float> Modifiers = new();
}
