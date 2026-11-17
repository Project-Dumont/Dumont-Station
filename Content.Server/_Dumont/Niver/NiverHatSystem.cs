// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Shared._Dumont.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Dumont.Niver;

/// <summary>
/// Puts a party hat on everyone who spawns, aghost too : )
/// </summary>
public sealed partial class NiverHatSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedStorageSystem _storage = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    private const string HeadSlot = "head";
    private const string BackSlot = "back";

    private static readonly List<EntProtoId> Hats =
    [
        "ClothingHeadHatPartyRed", "ClothingHeadHatPartyBlue", "ClothingHeadHatPartyGreen", "ClothingHeadHatPartyYellow"
    ];

    private bool _enabled;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        Subs.CVar(_cfg, DumontCVars.NiverEnabled, value => _enabled = value, true);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent ev)
    {
        GiveHat(ev.Mob);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var uid = ev.Entity;
        if (Prototype(uid)?.ID != GameTicker.AdminObserverPrototypeName.Id)
            return;

        if (_inventory.TryGetSlotEntity(uid, HeadSlot, out _))
            return;

        GiveHat(uid);
    }

    private void GiveHat(EntityUid uid)
    {
        if (!_enabled || !_inventory.HasSlot(uid, HeadSlot))
            return;

        if (_inventory.TryUnequip(uid, HeadSlot, out var oldHat, silent: true, force: true))
            StashItem(uid, oldHat.Value);

        var hat = SpawnAtPosition(_random.Pick(Hats), Transform(uid).Coordinates);
        if (!_inventory.TryEquip(uid, hat, HeadSlot, silent: true, force: true))
            StashItem(uid, hat);
    }

    private void StashItem(EntityUid uid, EntityUid item)
    {
        if (_inventory.TryGetSlotEntity(uid, BackSlot, out var bag) &&
            TryComp<StorageComponent>(bag, out var storage) &&
            _storage.Insert(bag.Value, item, out _, storageComp: storage, playSound: false))
            return;

        _hands.PickupOrDrop(uid, item, checkActionBlocker: false);
    }
}
