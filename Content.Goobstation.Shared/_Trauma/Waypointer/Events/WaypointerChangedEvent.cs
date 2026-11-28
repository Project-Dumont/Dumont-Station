// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Inventory;
// Dumont end

namespace Content.Trauma.Shared.Waypointer.Events;

/// <summary>
/// Whenever a clothing that shows waypointers is equipped.
/// </summary>
[ByRefEvent]
public record struct WaypointerChangedEvent() : IInventoryRelayEvent
{
    public HashSet<ProtoId<WaypointerPrototype>> Waypointers = [];
    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
}
