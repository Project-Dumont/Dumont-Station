// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Gabystation.Economy;
using Content.Shared._Gabystation.ATM;
using Content.Shared._Gabystation.NanoBank;
using Content.Shared.Cargo.Components;
using Content.Shared.Stacks;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;

namespace Content.Server._Gabystation.ATM;

public sealed partial class BankATMSystem : SharedBankATMSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly EconomyManagerSystem _economy = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BankATMComponent, BeforeActivatableUIOpenEvent>(UpdateInterfaceEvent);
        SubscribeLocalEvent<BankATMComponent, EntInsertedIntoContainerMessage>(UpdateInterfaceEvent);
        SubscribeLocalEvent<BankATMComponent, EntRemovedFromContainerMessage>(UpdateInterfaceEvent);

        Subs.BuiEvents<BankATMComponent>(BankATMUIKey.Key, subs =>
        {
            subs.Event<BankATMMessage>(OnMesssage);
        });

        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var ents = AllEntityQuery<BankATMComponent>();
        while (ents.MoveNext(out var uid, out var comp))
        {
            if (comp.Printing >= 0f)
            {
                comp.Printing -= frameTime;
                continue;
            }
            if (comp.MoneyToPrint <= 0)
                continue;

            var xform = Transform(uid);

            var cashId = SpawnAtPosition(comp.CashPrototype, xform.Coordinates);
            _stack.SetCount(cashId, comp.MoneyToPrint);
            comp.MoneyToPrint = 0;
        }
    }

    private void OnMesssage(EntityUid ent, BankATMComponent comp, BankATMMessage args)
    {
        var cardId = comp.CardSlot.Item;

        if (cardId is null 
            || !TryComp<NanoBankCardComponent>(cardId, out var card)
            || !ValidateCard((cardId.Value, card)) 
            || !TryComp<EconomyManagerComponent>(card?.Station, out var economy))
            return;

        switch (args.Type)
        {
            case BankATMMsgType.Deposit:
                HandleDeposit((ent, comp), (cardId.Value, card));
                break;
            case BankATMMsgType.Withdraw:
                HandleWithdraw((ent, comp), (cardId.Value, card), args.Amount);
                break;
            default:
                UpdateUi((ent, comp));
                break;
        }
    }

    private void HandleWithdraw(Entity<BankATMComponent> ent, Entity<NanoBankCardComponent> card, int? amount)
    {
        if (ent.Comp.Printing > 0f)
            return;

        if (amount is null || !TryComp<EconomyManagerComponent>(card.Comp.Station, out var economy))
            return;

        if (!_economy.TryGetData(economy, card.Comp.AccountId, out var data) 
            || data.Balance < amount
            || !_economy.TrySetBalance(economy, card.Comp.AccountId, (data.Balance - amount) ?? 0))
            return;

        _audio.PlayPvs(ent.Comp.PrintSound, ent.Owner);
        ent.Comp.MoneyToPrint = amount ?? 1;
        ent.Comp.Printing = 1f;

        var ev = new AccountTransferenceCompleted()
        {
            Type = TransferenceTypes.Withdraw,
            Account = data,
            AccountId = card.Comp.AccountId,
            Amount = amount ?? 1
        };
        RaiseLocalEvent(card.Comp.Station.Value, ev);

        UpdateUi(ent);
    }

    private void HandleDeposit(Entity<BankATMComponent> ent, Entity<NanoBankCardComponent> card)
    {
        if (!TryComp<EconomyManagerComponent>(card.Comp.Station, out var economy))
            return;

        var cashCount = 0;
        if (ent.Comp.CashSlot.HasItem && TryComp<StackComponent>(ent.Comp.CashSlot.Item, out var cash))
            cashCount = cash.Count;

        if (!_economy.TryGetData(economy, card.Comp.AccountId, out var data)
            || !_economy.TrySetBalance(economy, card.Comp.AccountId, data.Balance + cashCount))
            return;

        Del(ent.Comp.CashSlot.Item);
        _audio.PlayPvs(ent.Comp.DepositSound, ent.Owner);

        var ev = new AccountTransferenceCompleted()
        {
            Type = TransferenceTypes.Deposit,
            Account = data,
            AccountId = card.Comp.AccountId,
            Amount = cashCount
        };
        RaiseLocalEvent(card.Comp.Station.Value, ev);

        UpdateUi(ent);
    }

    // Handles update from events
    private void UpdateInterfaceEvent(EntityUid ent, BankATMComponent comp, EntityEventArgs args)
    {
        UpdateUi((ent, comp));
    }

    private void UpdateUi(Entity<BankATMComponent> ent, string? message = default)
    {
        int balance = 0;
        int accountId = 0000;
        bool hasCard = false;
        int? moneyInside = default;

        EntityUid? cardId = default;

        if (ent.Comp.CashSlot.HasItem && TryComp<StackComponent>(ent.Comp.CashSlot.Item, out var cash))
            moneyInside = cash.Count;

        if (ent.Comp.CardSlot.HasItem)
            cardId = ent.Comp.CardSlot.Item;

        if (cardId is not null && TryComp<NanoBankCardComponent>(cardId, out var card)
            && ValidateCard((cardId.Value, card)) && TryComp<EconomyManagerComponent>(card.Station, out var economy))
        {
            hasCard = true;
            _economy.TryGetBalance(economy, card.AccountId, out balance);
            accountId = card.AccountId;
        }

        _uiSystem.SetUiState(ent.Owner, BankATMUIKey.Key, new BankATMBuiState()
        {
            Balance = balance,
            HasCard = hasCard,
            MoneyInside = moneyInside,
            Message = message,
            AccountId = accountId,
        });
    }

    private bool ValidateCard(Entity<NanoBankCardComponent> ent)
    {
        if (!TryComp<EconomyManagerComponent>(ent.Comp.Station, out var economy))
            return false;

        if (!_economy.ValidateCard(economy, ent.Comp))
            return false;

        return ent.Comp.LoggedIn; // cards must be logged-in too
        // todo: log-in cards in ATM?
    }
}
