// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.Power.Components;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared._DV.NanoChat;
using Content.Shared.PDA;
using Content.Shared.Radio.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared._Gabystation.NanoBank;
using Content.Server._Gabystation.Economy;
using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using Content.Shared._Gabystation.CartridgeLoader.Cartridges;
using Robust.Server.Containers;
using Content.Shared.Mobs;
using Linguini.Syntax.Ast;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Utility;

namespace Content.Server._Gabystation.CartridgeLoader.Cartridges;

public sealed class NanoBankCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly EconomyManagerSystem _economy = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedNanoBankSystem _nanoBank = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        //SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
        SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<EconomyManagerComponent, AccountTransferenceCompleted>(OnTransference);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Update card references for any cartridges that need it
        var query = EntityQueryEnumerator<NanoBankCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var nanoBank, out var cartridge))
        {
            if (cartridge.LoaderUid == null)
                continue;

            // Check if we need to update our card reference
            if (!TryComp<PdaComponent>(cartridge.LoaderUid, out var pda))
                continue;

            var newCard = pda.ContainedId;
            var currentCard = nanoBank.Card;

            // If the cards match, nothing to do
            if (newCard == currentCard)
                continue;

            // Update card reference
            nanoBank.Card = newCard;

            // Update UI state since card reference changed
            UpdateUI((uid, nanoBank), cartridge.LoaderUid.Value);
        }
    }

    private void OnUiReady(Entity<NanoBankCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        _cartridge.RegisterBackgroundProgram(args.Loader, ent);
        UpdateUI(ent, args.Loader);
    }

    private void OnMessage(Entity<NanoBankCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not NanoBankUiMessageEvent msg)
            return;

        var loaderId = GetEntity(args.LoaderUid);
        if (!GetCardEntity(loaderId, out var card))
            return;

        switch (msg.Type)
        {
            case NanoBankUiMessageType.Logout:
                _nanoBank.LogoutId(card.AsNullable());
                break;
            case NanoBankUiMessageType.Login:
                HandleLogin(card, msg.TargetAccount, msg.Content);
                break;

            case NanoBankUiMessageType.ToggleMute:
                HandleToggleMute(card);
                break;

            case NanoBankUiMessageType.Transfer:
                HandleTransfer(card, msg.TargetAccount, msg.Content);
                break;
        }

        UpdateUI(ent, loaderId);
    }

    private void OnTransference(Entity<EconomyManagerComponent> ent, ref AccountTransferenceCompleted args)
    {
        // Not the best way lol
        var ents = AllEntityQuery<NanoBankCardComponent>();
        while (ents.MoveNext(out var uid, out var comp))
        {
            if (!comp.LoggedIn || comp.AccountPin is 0)
                continue;

            if (!_economy.ValidateLogin(ent.Comp, comp.AccountId, comp.AccountPin))
                continue;

            if (!comp.NotificationsMuted)
                HandleNotification(ent, (uid, comp), ref args);

            UpdateUIForCard(uid);
        }
    }

    private void HandleNotification(Entity<EconomyManagerComponent> ent, Entity<NanoBankCardComponent> card, ref AccountTransferenceCompleted args)
    {
        switch (args.Type)
        {
            case TransferenceTypes.Payment:
                if (card.Comp.AccountId == args.AccountId)
                    HandleNotification(card.Owner, "economy-notification-payment-title", "economy-notification-payment-body", args.Amount);
                break;
            case TransferenceTypes.Transference:
                if (card.Comp.AccountId == args.AccountId) // we are the sender
                    HandleNotification(card.Owner, "economy-notification-transference-sender-title",
                        "economy-notification-transference-sender-body", args.Amount, ("target", $"#{args.TargetAccount:D4}"));
                else if (card.Comp.AccountId == args.TargetAccount) // we are the receiver
                    HandleNotification(card.Owner, "economy-notification-transfer-target-title",
                        "economy-notification-transfer-target-body", args.Amount, ("name", args.Account?.OwnerName ?? "?"));
                break;
            /*case TransferenceTypes.Purchase: // Can be annoying in a prod. round
                if (card.Comp.AccountId == args.AccountId)
                    HandleNotification(card.Owner, "economy-notification-purchase-title", "economy-notification-purchase-body", args.Amount);
                break;*/
            case TransferenceTypes.Withdraw:
                if (card.Comp.AccountId == args.AccountId)
                    HandleNotification(card.Owner, "economy-notification-withdraw-title", "economy-notification-withdraw-body", args.Amount);
                break;
            case TransferenceTypes.Deposit:
                if (card.Comp.AccountId == args.AccountId)
                    HandleNotification(card.Owner, "economy-notification-deposit-title", "economy-notification-deposit-body", args.Amount);
                break;
        }

    }

    /// <summary>
    ///     Gets the ID card entity associated with a PDA.
    /// </summary>
    /// <returns>True if a valid NanoBank card was found</returns>
    private bool GetCardEntity(Entity<PdaComponent?> pda,
        [NotNullWhen(true)] out Entity<NanoBankCardComponent> card)
    {
        var (pdaUid, pdaComp) = pda;
        card = default;

        if (!Resolve(pdaUid, ref pdaComp, false) ||
            pdaComp.ContainedId is not { } cardUid ||
            !TryComp<NanoBankCardComponent>(cardUid, out var cardComp))
            return false;

        card = (cardUid, cardComp);
        return true;
    }

    private bool TryPdaFromId(EntityUid idUid,
        [NotNullWhen(true)] out Entity<PdaComponent> pda)
    {
        pda = default;

        if (!_container.TryGetContainingContainer(idUid, out var container)
            || !TryComp<PdaComponent>(container.Owner, out var pdaComp))
            return false;

        pda = (container.Owner, pdaComp);
        return true;
    }

    private void HandleNotification(EntityUid uid, string tittleLoc, string bodyLoc, int? amount = default, (string, object)? arg = default)
    {
        if (!TryPdaFromId(uid, out var pda))
            return;

        var args = new List<(string, object)>();

        if (amount is not null)
            args.Add(("amount", amount.Value));

        if (arg is not null)
            args.Add(arg.Value);

        string body = Loc.GetString(bodyLoc, args.ToArray());

        _cartridge.SendNotification(pda.Owner, Loc.GetString(tittleLoc), body);
    }

    private void HandleToggleMute(Entity<NanoBankCardComponent> card)
    {
        var foo = (card, card.Comp);
        _nanoBank.SetNotificationsMuted(foo, !_nanoBank.GetNotificationsMuted(foo));
        UpdateUIForCard(card);
    }

    private void HandleLogin(Entity<NanoBankCardComponent> card, int? id, int? pin)
    {
        if (!TryComp<EconomyManagerComponent>(card.Comp.Station, out var economy))
            return;

        if (id is null || pin is null)
            return;

        if (_economy.ValidateLogin(economy, id.Value, pin.Value))
        {
            card.Comp.LoggedIn = true;
            card.Comp.AccountId = id.Value;
            card.Comp.AccountPin = pin.Value;
        }
        else
            card.Comp.LoggedIn = false;

        Dirty(card);
        UpdateUIForCard(card);
    }

    private void HandleTransfer(Entity<NanoBankCardComponent> card, int? targetAcc, int? amount)
    {
        if (targetAcc is null
            || amount is null
            || card.Comp.Station is null
            || !TryComp<EconomyManagerComponent>(card.Comp.Station, out var economy)
            || !_economy.TransferBalance((card.Comp.Station.Value, economy), targetAcc.Value, card.Comp.AccountId, amount.Value))
        {
            HandleNotification(card.Owner, "economy-notification-transference-failed-title", "economy-notification-transference-failed-body", default);
            return;
        }

        //HandleNotification(card.Owner, "economy-notification-transfer-title", "economy-notification-transfer-body", amount);
        //UpdateUIForCard(card);
    }

    // Talvez isso não devese ser público. Mas preciso chamar em PdaSystem.
    public void UpdateUIForCard(EntityUid cardUid)
    {
        // Os UpdateUI devem ser relativos ao accountId.
        if (!TryPdaFromId(cardUid, out var pda)
            || !_container.TryGetContainer(pda.Owner, SharedCartridgeLoaderSystem.InstalledContainerId, out var container))
            return;

        var maybeNanoBankUid = container.ContainedEntities
            .Where(HasComp<NanoBankCartridgeComponent>) // Pode acontecer do PDA ter mais de um nanobank instalado?
            .FirstOrNull();

        if (maybeNanoBankUid is not { } nanoBankUid
            || !TryComp<NanoBankCartridgeComponent>(nanoBankUid, out var nanoBankComp))
            return;

        UpdateUI((nanoBankUid, nanoBankComp), pda);

        // Find any PDA containing this card and update its UI
        // var query = EntityQueryEnumerator<NanoBankCartridgeComponent, CartridgeComponent>();
        // while (query.MoveNext(out var uid, out var comp, out var cartridge))
        // {
        //     if (comp.Card != cardUid || cartridge.LoaderUid == null)
        //         continue;

        //     UpdateUI((uid, comp), cartridge.LoaderUid.Value);
        // }
    }

    private void UpdateUI(Entity<NanoBankCartridgeComponent> nanoBank, EntityUid loader)
    {
        int accountId = 0;
        int pin = 0;
        bool notificationsMuted = false;
        bool logged = false;
        float nextPayment = 0;
        int balance = 0;

        NanoBankCardComponent? card = default;

        if (nanoBank.Comp.Card != null && TryComp(nanoBank.Comp.Card, out card))
        {
            // Se o PDA tem ID, então puxa as informações bancárias do ID
            accountId = card.AccountId;
            pin = card.AccountPin;
            notificationsMuted = card.NotificationsMuted;
            logged = card.LoggedIn;
        }
        if (logged && card?.Station is not null && TryComp<EconomyManagerComponent>(card?.Station, out var economy))
        {
            // validate log-in
            logged = _economy.ValidateLogin(economy, accountId, pin);
            card.LoggedIn = logged;
            nextPayment = economy.PaymentCooldownRemaining;
            _economy.TryGetBalance(economy, accountId, out balance);
        }

        var state = new NanoBankUiState(accountId,
            pin,
            notificationsMuted,
            logged,
            nextPayment,
            balance);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
