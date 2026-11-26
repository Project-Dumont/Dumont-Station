using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Maid.GameTicking.Prototypes;

[Prototype]
public sealed partial class AnimatedLobbyScreenPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField(required: true)]
    public ResPath Background = default!;

    [DataField]
    public string? Name;

    [DataField]
    public string? Artist;
}
