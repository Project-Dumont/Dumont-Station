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

namespace Content.Trauma.Common.Damage;

/// <summary>
/// Raised on an entity taking damage from the HealthChangeEntityEffect system.
/// </summary>
[ByRefEvent]
public record struct OnHealthChangeEvent(DamageSpecifier Damage);
