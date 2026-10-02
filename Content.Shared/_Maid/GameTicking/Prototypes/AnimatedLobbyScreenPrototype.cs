using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Maid.GameTicking.Prototypes;

[Prototype]
public sealed partial class AnimatedLobbyScreenPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("background", required: true)]
    public ResPath Path;

    [DataField]
    public string? Name;

    [DataField]
    public string? Artist;
}
