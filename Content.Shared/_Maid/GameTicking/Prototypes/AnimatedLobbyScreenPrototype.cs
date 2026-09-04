using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Maid.GameTicking.Prototypes;

[Prototype]
[NetSerializable, Serializable]
public sealed partial class AnimatedLobbyScreenPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("background", required: true)]
    public ResPath Path;

    [DataField]
    public string? Name;

    [DataField]
    public string? Artist;
}
