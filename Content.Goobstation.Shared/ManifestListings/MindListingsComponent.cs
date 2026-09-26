// Dumont start
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
// Dumont end
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.ManifestListings;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindListingsComponent : Component
{
    [DataField]
    public Dictionary<int, List<ListingData>> Listings = new();

    // Dumont start
    [DataField]
    public Dictionary<int, Dictionary<string, Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2>>> Spent = new();
    // Dumont end

    [DataField]
    public SpriteSpecifier.Texture DefaultTexture = new(new ResPath("/Textures/Interface/Actions/shop.png"));
}
