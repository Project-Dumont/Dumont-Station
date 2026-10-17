// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Containers.ItemSlots;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the target body part to have something inside its cavity.
/// Parts without a cavity always fail.
/// </summary>
public sealed partial class CavityHasItem : AbductorTaskCondition
{
    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent<BodyPartComponent>(target, out var part) ||
            !entMan.System<ItemSlotsSystem>().TryGetSlot(target, part.ContainerName, out var slot))
            return false;

        return slot.HasItem;
    }
}
