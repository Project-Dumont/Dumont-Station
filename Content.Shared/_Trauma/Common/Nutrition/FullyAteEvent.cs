// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.Nutrition;

/// <summary>
/// Raised on the mob that ate food just before it gets deleted.
/// User is different from the event target if being force fed.
/// </summary>
[ByRefEvent]
public readonly record struct FullyAteEvent(EntityUid Food, EntityUid User);
