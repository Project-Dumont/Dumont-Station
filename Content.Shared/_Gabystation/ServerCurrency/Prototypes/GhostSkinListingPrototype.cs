using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Gabystation.ServerCurrency.Prototypes;

[Prototype("ghostSkinListing")]
public sealed class GhostSkinListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField(required: true)]
    public int Price { get; private set; } = 0;

    [DataField(required: true)]
    public SpriteSpecifier Sprite { get; private set; } = default!;

    [DataField(required: false)]
    public string? Color { get; private set; } = null;

    [DataField(required: false)]
    public Vector2 Scale { get; private set; } = new(1, 1);

    [DataField(required: false)]
    public bool Available { get; private set; } = true;
}
