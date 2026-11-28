// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.Throwing;

/// <summary>
/// Raised on an entity whose speed is about to be modified by something throwing it.
/// </summary>
[ByRefEvent]
public record struct ModifyThrownSpeedEvent(EntityUid User, float BaseThrowSpeed, float Distance);
