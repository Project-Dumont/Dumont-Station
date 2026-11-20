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
/// Raised on an entity that is about to be thrown to allow preventing it.
/// </summary>
[ByRefEvent]
public record struct BeingThrownAttemptEvent(bool Cancelled = false);
