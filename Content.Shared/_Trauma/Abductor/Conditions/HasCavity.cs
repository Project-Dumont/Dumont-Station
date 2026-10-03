// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Containers.ItemSlots;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the target body part to have a cavity slot at all.
/// </summary>
public sealed partial class HasCavity : AbductorTaskCondition
{
    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        return entMan.TryGetComponent<BodyPartComponent>(target, out var part) &&
            entMan.System<ItemSlotsSystem>().TryGetSlot(target, part.ContainerName, out _);
    }
}
