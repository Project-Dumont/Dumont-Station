namespace Content.Server._Dumont.Niver;

[RegisterComponent]
public sealed partial class NiverSpawnComponent : Component
{
    [DataField]
    public List<string> PresentPrototypes = [
        "PresentRandom", "PresentRandomCoal", "PresentRandomCash", "PresentRandomUnsafe" // Tenha medo.
    ];

    [DataField]
    public List<string> BalloonPrototypes = [
        "RedBalloon", "BlueBalloon", "GreenBalloon", "YellowBalloon", "PinkBalloon",
        "BlackBalloon", "WhiteBalloon", "ZebraBalloon", "SteelBalloon", "RainbowBalloon",
        "CeramicBalloon"
    ];

    [DataField]
    public string ConfettiDecal = "Confetti";

    [DataField]
    public float ConfettiChance = 0.25f;
}

[RegisterComponent]
public sealed partial class NiverSpawnBlacklistComponent : Component
{
}