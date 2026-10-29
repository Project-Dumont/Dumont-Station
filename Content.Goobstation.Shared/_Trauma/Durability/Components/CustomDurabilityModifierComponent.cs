// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Durability.Components;

/// <summary>
/// Added to repair materials to determine how they will modify weapon damage, such as when reinforced
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CustomDurabilityModifierComponent : Component
{
    /// <summary>
    /// State -> (max flat modifier, max percent modifier)
    /// When calculating result modifier, it takes either +flat or *percent,
    /// whichever is less (or more if damage is reduced)
    /// </summary>
    [DataField(required: true)]
    public Dictionary<DurabilityState, Vector2> MaxDurabilityStateModifiers;
}
