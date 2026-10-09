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

public abstract partial class BaseSpriteOverlayComponent : Component
{
    public abstract Enum Key { get; set; }

    public abstract SpriteSpecifier? Sprite { get; set; }

    public virtual bool Unshaded { get; set; } = true;

    public virtual Vector2 Offset { get; set; } = Vector2.Zero;

    public virtual Color Color { get; set; } = Color.White;

    public virtual bool Active { get; set; } = true;
}
