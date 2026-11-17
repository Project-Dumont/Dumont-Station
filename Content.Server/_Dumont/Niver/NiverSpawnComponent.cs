using Content.Shared.Decals;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Niver;

[RegisterComponent]
public sealed partial class NiverSpawnComponent : Component
{
    [DataField]
    public List<EntProtoId> PresentPrototypes = [
        "PresentRandom", "PresentRandomCoal", "PresentRandomCash", "PresentRandomUnsafe" // Tenha medo.
    ];

    [DataField]
    public List<EntProtoId> BalloonPrototypes = [
        "RedBalloon", "BlueBalloon", "GreenBalloon", "YellowBalloon", "PinkBalloon",
        "BlackBalloon", "WhiteBalloon", "ZebraBalloon", "SteelBalloon", "RainbowBalloon",
        "CeramicBalloon"
    ];

    [DataField]
    public ProtoId<DecalPrototype> ConfettiDecal = "Confetti";

    [DataField]
    public float ConfettiChance = 0.25f;
}

/// <summary>
/// Put on a station, grid or map to skip the birthday spawns there.
/// </summary>
[RegisterComponent]
public sealed partial class NiverSpawnBlacklistComponent : Component
{
}
