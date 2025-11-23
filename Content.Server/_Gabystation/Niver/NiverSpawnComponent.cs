namespace Content.Server._Gabystation.Niver;

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
}
