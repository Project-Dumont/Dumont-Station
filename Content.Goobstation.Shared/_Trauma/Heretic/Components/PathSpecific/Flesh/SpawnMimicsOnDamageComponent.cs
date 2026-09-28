// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Maths.FixedPoint;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Flesh;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnMimicsOnDamageComponent : Component
{
    [DataField]
    public float DamageChanceRatio = 0.01f;

    [DataField]
    public FixedPoint2 MinDamage = 5;

    [DataField]
    public float GhoulHealthMultiplier = 0.5f;

    [DataField]
    public FixedPoint2 BaseGhoulHealth = 25;

    [DataField]
    public bool GiveBlade = true;

    [DataField]
    public bool MakeGhostRole = true;
}
