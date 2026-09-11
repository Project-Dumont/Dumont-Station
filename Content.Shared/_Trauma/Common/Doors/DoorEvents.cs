// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.Doors;

[ByRefEvent]
public record struct ShouldDoorCrushEvent(bool ShouldCrush, TimeSpan CrushDelay);

[ByRefEvent]
public record struct DoorOpenedEvent(EntityUid Door, EntityUid? User);
