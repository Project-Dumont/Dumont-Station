// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;
using Robust.Shared.Map;
// Dumont end

namespace Content.Trauma.Common.Heretic;


[ByRefEvent]
public record struct ParentPacketReceiveAttemptEvent(bool Cancelled = false);

[ByRefEvent]
public record struct GetVirtualItemBlockingEntityEvent(EntityUid Uid);

[ByRefEvent]
public record struct BeforeAccessReaderCheckEvent(bool Cancelled = false);

[ByRefEvent]
public record struct BeforeHolosignUsedEvent(EntityUid User, EntityCoordinates ClickLocation, bool Handled = false, bool Cancelled = false);

[ByRefEvent]
public readonly record struct IconSmoothCornersInitializedEvent;

[ByRefEvent]
public record struct ValidateInstantWorldTargetActionEvent(EntityUid User, EntityUid Provider, bool Result = false);

[ByRefEvent]
public readonly record struct TryPerformInstantWorldTargetActionEvent;

[ByRefEvent]
public readonly record struct ConsumingFoodEvent(EntityUid Food, FixedPoint2 Volume);

[ByRefEvent]
public record struct BeforeSpawnPullingVirtualItemsEvent(EntityUid Puller, EntityUid Pulled, bool Cancelled = false);

[ByRefEvent]
public record struct GetGrabMovespeedEvent(float Speed);

[ByRefEvent]
public record struct CanStandWhileImmobileEvent(bool CanStand = false);

[ByRefEvent]
public record struct BeforeMovespeedModifierAppliedEvent(float WalkModifier, float SprintModifier) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

[ByRefEvent]
public record struct GetExamineRangeEvent(float Range);

[ByRefEvent]
public record struct ShouldBlockContextMenuEvent(EntityUid Target, bool ShouldBlock = false);

[ByRefEvent]
public readonly record struct FireStacksChangedEvent(EntityUid Uid, float FireStacks);

[ByRefEvent]
public record struct GetFirestackPassiveModifierEvent(bool OnFire, bool Resisting, float Modifier) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.OUTERCLOTHING;
}

[ByRefEvent]
public record struct ShouldExtinguishInSpaceEvent(bool Cancelled = false) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.OUTERCLOTHING;
}

[ByRefEvent]
public record struct NoFirestacksUpdateEvent(EntityUid Uid, bool Handled = false) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.OUTERCLOTHING;
}

[ByRefEvent]
public record struct CanSeeOnCameraEvent(EntityUid Uid, bool Cancelled = false) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.OUTERCLOTHING;
}
