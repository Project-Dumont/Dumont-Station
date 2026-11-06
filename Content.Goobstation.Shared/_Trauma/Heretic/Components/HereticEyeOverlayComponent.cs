// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticEyeOverlayComponent : BaseSpriteOverlayComponent
{
    public override Enum Key { get; set; } = HereticEyeOverlayKey.Key;

    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "heretic_eye_dripping");

    [DataField]
    public override Vector2 Offset { get; set; } = new(0f, 0.5f);
}

public enum HereticEyeOverlayKey : byte
{
    Key,
}
