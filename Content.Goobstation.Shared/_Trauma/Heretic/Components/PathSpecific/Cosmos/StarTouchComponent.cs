// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchComponent : Component
{
    [DataField]
    public TimeSpan DrowsinessTime = TimeSpan.FromSeconds(8);

    [DataField]
    public SpriteSpecifier BeamSprite = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "cosmic_beam");

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8);

    [DataField]
    public float Range = 8f;

    [DataField]
    public float CosmicFieldLifetime = 30f;
}
