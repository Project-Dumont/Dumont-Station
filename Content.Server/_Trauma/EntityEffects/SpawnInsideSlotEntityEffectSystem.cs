// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Trauma.Shared.EntityEffects;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class SpawnInsideSlotEntityEffectSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedStorageSystem _storage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<SpawnInsideSlot>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<SpawnInsideSlot> args)
    {
        var uid = args.Args.TargetEntity;
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        var quantity = args.Effect.Number;
        var slotId = args.Effect.Slot;
        var proto = args.Effect.Entity;
        var coords = Transform(uid).Coordinates;

        var target = uid;
        if (_inventory.TryGetSlotContainer(uid, slotId, out var slot, out _, inventory: inventory) && slot.ContainedEntity is {} item)
            target = item;
        var storage = CompOrNull<StorageComponent>(target);

        for (var i = 0; i < quantity; i++)
        {
            var spawned = SpawnAtPosition(proto, coords);
            if (storage != null)
                _storage.Insert(target, spawned, out _, storageComp: storage, playSound: false);
        }
    }
}
