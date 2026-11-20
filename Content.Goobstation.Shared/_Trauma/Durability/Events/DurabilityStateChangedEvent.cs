// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Durability.Events;

// Raised on Weapon
[ByRefEvent]
public record struct DurabilityStateChangedEvent(DurabilityState OldState, DurabilityState NewState, EntityUid Weapon, EntityUid? Attacker = null, HashSet<EntityUid>? Targets = null, EntityUid? Used = null);

// Raised on Used
[ByRefEvent]
public record struct DurabilityStateChangedByEvent(DurabilityState OldState, DurabilityState NewState, EntityUid Weapon, EntityUid? Attacker = null, HashSet<EntityUid>? Targets = null, EntityUid? Used = null);
