// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Sprite;

/// <summary>
/// Controls sprite visibility, used to avoid conflicts for different systems/overlays modifying alpha
/// </summary>
[RegisterComponent]
public sealed partial class SpriteVisibilityComponent : Component
{
    /// <summary>
    /// Source key -> alpha value [0, 1)
    /// Final alpha is calculated by multiplying the values
    /// If final alpha is 0, sprite.Visible is set to false
    /// </summary>
    [DataField]
    public Dictionary<string, float> VisibilityModifiers = new();
}
