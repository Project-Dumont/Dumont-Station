// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Goobstation.Shared.Emoting;

[RegisterComponent]
public sealed partial class ShowSpriteLayerStatusEffectComponent : Component
{
    [DataField(required: true)]
    public Enum Layer = default!;

    [DataField]
    public bool SetVisible = true;
}

[Flags, Serializable, NetSerializable]
public enum HumanoidVisualEmoteLayers : byte
{
    None = 0,
    Sigh = 1 << 0,
    Cry = 1 << 1,
    Blush = 1 << 2,
    Tongue = 1 << 3,
}
