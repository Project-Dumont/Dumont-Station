// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

using Robust.Shared.Utility;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side;

/// <summary>
/// Adds visual mark for target if we are aiming at them using <see cref="AimedRifleComponent"/>\
/// This is visible to everyone
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AimedRifleMarkerComponent : BaseSpriteOverlayComponent
{
    public override Enum Key { get; set; } = LionhunterAimMarkerKey.Key;

    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "sniper_zoom");
}

public enum LionhunterAimMarkerKey : byte
{
    Key,
}
