// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Trauma.Shared.Durability.Events;

[ByRefEvent]
public record struct DurabilityChangeAttemptEvent(EntityUid Uid, FixedPoint2 Damage);

[ByRefEvent]
public record struct DurabilityDamageChangedEvent(EntityUid Uid, FixedPoint2 Damage, FixedPoint2 OldDamage);
