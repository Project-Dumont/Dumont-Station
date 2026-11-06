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
public sealed partial class HereticAuraComponent : BaseSpriteOverlayComponent
{
    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "heretic_aura");

    public override Enum Key { get; set; } = HereticAuraKey.Key;

    public override bool Unshaded { get; set; } = false;
}

public enum HereticAuraKey : byte
{
    Key,
}
