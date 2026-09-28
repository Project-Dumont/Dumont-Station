// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Flesh;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshSurgeryComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    [DataField]
    public float AreaHealRange = 7f;

    [DataField]
    public EntProtoId KnitFleshEffect = "KnitFleshEffect";

    [DataField]
    public TimeSpan MaxAreaCooldown = TimeSpan.FromSeconds(40);
}
