// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Trauma.Shared.EntityEffects;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class EquipEntityTableSystem : EntitySystem
{
    [Dependency] private EntityTableSystem _table = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<EquipEntityTable>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<EquipEntityTable> args)
    {
        var uid = args.Args.TargetEntity;
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        var e = args.Effect;
        foreach (var id in _table.GetSpawns(e.Table))
        {
            var item = SpawnNextToOrDrop(id, uid); // Dumont
            if (!_inventory.TryEquip(uid, item, e.Slot, silent: e.Silent, inventory: inventory))
                _hands.TryPickupAnyHand(uid, item);
        }
    }
}
