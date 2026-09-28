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

namespace Content.Trauma.Common.Throwing;

/// <summary>
/// Raised on thrown object before it deals damage to target
/// </summary>
[ByRefEvent]
public record struct BeforeDamageOtherOnHitEvent(EntityUid? User,
    EntityUid Target,
    DamageSpecifier BaseDamage,
    DamageSpecifier BonusDamage,
    bool Cancelled = false);
