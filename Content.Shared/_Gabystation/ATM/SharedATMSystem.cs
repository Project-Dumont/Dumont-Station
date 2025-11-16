// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;

namespace Content.Shared._Gabystation.ATM;

public abstract partial class SharedBankATMSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BankATMComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BankATMComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<BankATMComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<BankATMComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        string? markup = default;

        if (ent.Comp.CardSlot.HasItem)
            markup = Loc.GetString("bank-atm-has-card");

        if (ent.Comp.CashSlot.HasItem)
            markup = Loc.GetString("bank-atm-has-cash");

        if (ent.Comp.CardSlot.HasItem && ent.Comp.CashSlot.HasItem)
            markup = Loc.GetString("bank-atm-has-card-cash");

        if (markup is not null)
            args.PushMarkup(markup);
    }

    private void OnComponentInit(EntityUid uid, BankATMComponent comp, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, BankATMComponent.CashSlotId, comp.CashSlot);
        _itemSlotsSystem.AddItemSlot(uid, BankATMComponent.CardSlotId, comp.CardSlot);
    }

    private void OnComponentRemove(EntityUid uid, BankATMComponent comp, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, comp.CashSlot);
        _itemSlotsSystem.RemoveItemSlot(uid, comp.CardSlot);
    }
}
