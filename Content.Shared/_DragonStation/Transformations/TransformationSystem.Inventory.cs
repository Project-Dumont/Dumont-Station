using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    private void InitializeInventory()
    {
        SubscribeLocalEvent<TransformationComponent, IsEquippingTargetAttemptEvent>(OnEquipAttempt);
    }

    // Some transformations reject protective gear so the form itself is the protection.
    private void OnEquipAttempt(EntityUid uid, TransformationComponent component, ref IsEquippingTargetAttemptEvent args)
    {
        if (!component.Active || !component.BlockArmorSlots)
            return;

        if (args.Slot is not ("outerClothing" or "head" or "suitstorage"))
            return;

        args.Cancel();
        args.Reason = component.BlockedGearReason;
    }

    // Drop incompatible worn gear when the form activates.
    private void StripBlockedGear(EntityUid uid, TransformationComponent component)
    {
        if (!component.BlockArmorSlots)
            return;

        _inventory.TryUnequip(uid, "suitstorage", silent: true, force: true);
        _inventory.TryUnequip(uid, "head", silent: true, force: true);
        _inventory.TryUnequip(uid, "outerClothing", silent: true, force: true);
    }
}
