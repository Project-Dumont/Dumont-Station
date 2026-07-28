using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.ServerCurrency.Prototypes;

[Prototype]
public sealed class TitleListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Title { get; private set; } = string.Empty;

    [DataField(required: true)]
    public int Price { get; private set; } = 0;

    [DataField(required: false)]
    public string? Color { get; private set; } = null;

    [DataField(required: false)]
    public bool Available { get; private set; } = true;
}
