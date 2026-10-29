// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Lavaland.Common.Weapons.Ranged;

/// <summary>
/// Raised on a gun when a projectile has been fired by it.
/// </summary>
[ByRefEvent]
public record struct GunShotProjectileEvent(EntityUid FiredProjectile, EntityUid? User);
