// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side.Carvings;

[RegisterComponent, NetworkedComponent]
public sealed partial class MadCarvingComponent : Component
{
    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public TimeSpan MuteTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan BlindnessTime = TimeSpan.FromSeconds(10);
}
