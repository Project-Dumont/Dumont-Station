// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

[RegisterComponent, NetworkedComponent]
public sealed partial class SilverMaelstromComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public EntProtoId Status = "SilverMaelstromStatusEffect";

    [DataField]
    public float ExtraDamageMultiplier = 0.5f;

    [DataField]
    public float LifestealHealMultiplier = 0.25f;
}
