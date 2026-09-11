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
public sealed partial class SpriteRandomOffsetComponent : Component
{
    [DataField]
    public float MinX = -0.25f;

    [DataField]
    public float MaxX = 0.25f;

    [DataField]
    public float MinY = -0.25f;

    [DataField]
    public float MaxY = 0.25f;
}

[Serializable, NetSerializable]
public enum OffsetVisuals : byte
{
    Offset
}
