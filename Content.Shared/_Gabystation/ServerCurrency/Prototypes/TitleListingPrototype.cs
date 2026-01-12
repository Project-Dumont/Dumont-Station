using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.ServerCurrency.Prototypes;

[Prototype("titleListing")]
public sealed class TitleListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    [DataField("title", required: true)]
    public string Title { get; private set; } = string.Empty;

    [DataField("price", required: true)]
    public int Price { get; private set; } = 0;

    [DataField("color", required: false)]
    public string? Color { get; private set; } = null;

    [DataField("avaible", required: false)]
    public bool Avaible { get; private set; } = true; //todo this & comand giveTitle
}
