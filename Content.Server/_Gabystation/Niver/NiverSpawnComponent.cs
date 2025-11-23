// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
