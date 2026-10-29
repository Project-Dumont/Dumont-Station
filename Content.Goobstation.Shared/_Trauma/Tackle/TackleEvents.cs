// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.Inventory;

namespace Content.Trauma.Shared.Tackle;

[ByRefEvent]
public record struct TackleEvent(EntityUid User, List<TackleModifier> Sources) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

[ByRefEvent]
public record struct CalculateTackleModifierEvent(float Modifier = 0f, bool CanTackle = true) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
