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
/// Raised on an entity after it has taken immediate stamina damage.
/// Overtime stamina damage from batong etc is not counted.
/// </summary>
[ByRefEvent]
public record struct TookStaminaDamageEvent(EntityUid Target, EntityUid? Source, float Amount);
