// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.Damage;

/// <summary>
/// Raised on the mob holding a DamageOnHolding item and its gloves to prevent being damaged.
/// </summary>
[ByRefEvent]
public record struct DamageOnHoldingAttemptEvent(EntityUid Source, bool Cancelled = false);
