// SPDX-FileCopyrightText: 2025 CrimeMoot <wakeafa@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ganimed.SeedDna.Components;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;

namespace Content.Shared._Ganimed.SeedDna.System;

[UsedImplicitly]
public abstract class SharedSeedDnaConsoleSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeedDnaConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SeedDnaConsoleComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(EntityUid uid, SeedDnaConsoleComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, SeedDnaConsoleComponent.SeedSlotId, component.SeedSlot);
        _itemSlotsSystem.AddItemSlot(uid, SeedDnaConsoleComponent.DnaDiskSlotId, component.DnaDiskSlot);
    }

    private void OnComponentRemove(EntityUid uid, SeedDnaConsoleComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.SeedSlot);
        _itemSlotsSystem.RemoveItemSlot(uid, component.DnaDiskSlot);
    }
}
