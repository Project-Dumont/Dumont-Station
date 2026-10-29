// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Crucible.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WoundedSoldierComponent : Component
{
    [DataField]
    public float LifeStealMultiplier = 0.3f;

    [DataField]
    public float StaminaHealMultiplier = 0.5f;

    [DataField]
    public float OvertimeDamageThresholdRatio = 0.1f;

    [DataField]
    public DamageSpecifier DamageOverTime = new()
    {
        DamageDict =
        {
            { "Heat", 10 },
        },
    };

    [DataField]
    public LocId ExamineLoc = "wounded-solider-effect-examine-message";
}
