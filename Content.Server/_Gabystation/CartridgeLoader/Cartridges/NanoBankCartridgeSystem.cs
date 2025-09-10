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

namespace Content.Server._Gabystation.CartridgeLoader.Cartridges;

public sealed class NanoBankCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly EconomyManagerSystem _economy = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedNanoBankSystem _nanoBank = default!;

    // TODO list
    /// Evento pra quando receber transferencia
    /// Notificação pra quando receber transferencia
    /// Geração de senha

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        //SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
        SubscribeLocalEvent<NanoBankCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<AccountPaymentCompleted>(OnPayment);
        SubscribeLocalEvent<PdaComponent, EntRemovedFromContainerMessage>(OnIdRemoved);
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

    private void OnIdRemoved(Entity<PdaComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != "PDA-id")
            return;

        UpdateUIForCard(args.Entity);
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
                _nanoBank.LogoutId((loaderId, card));
                break;
            case NanoBankUiMessageType.Login:
                HandleLogin(card, msg.TargetAccount, (int?) msg.Content);
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

    public void OnPayment(AccountPaymentCompleted args)
    {
        //! Isso provavelmente tem um alto custo computacional, mas eu não sei outro jeito de fazer isso.
        // TODO: Novo metodo, o id recebe a mensagem e verifica se está na conta bancaria, se sim, envia uma outra mensagem pra cá.
        Log.Debug("Payment 1");
        var ents = AllEntityQuery<NanoBankCardComponent>();
        while (ents.MoveNext(out var uid, out var comp))
        {

            var station = _station.GetOwningStation(uid);
            if (!comp.LoggedIn || comp.AccountId is 0 || comp.AccountPin is 0 || station is null)
                continue;

            UpdateUIForCard(uid);

            if (comp.NotificationsMuted) // We dont need to computate this if notifications are disabled
                return; //? If u are planning do somemore here, put this if before HandleNotification

            if (!TryComp<EconomyManagerComponent>(station, out var economyComp))
                continue;

            if (!_economy.ValidateLogin(economyComp, comp.AccountId, comp.AccountPin))
                continue;

            HandleNotification(uid, "economy-notification-payment-title", "economy-notification-payment-body", args.Payment);
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
            pdaComp.ContainedId == null ||
            !TryComp<NanoBankCardComponent>(pdaComp.ContainedId, out var cardComp))
            return false;

        card = (pdaComp.ContainedId.Value, cardComp);
        return true;
        /// se eu reusar isso, preciso pegar o id que estiver na mao primeiro e nao so no pda
        /// pra coisas como atm
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

    private void HandleNotification(EntityUid uid, string tittleLoc, string bodyLoc, float? amount)
    {
        if (!TryPdaFromId(uid, out var pda))
            return;

        string body;

        if (amount is not null)
            body = Loc.GetString(bodyLoc, ("amount", amount));
        else
            body = Loc.GetString(bodyLoc);

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

    private void HandleTransfer(Entity<NanoBankCardComponent> card, int? targetAcc, float? amount)
    {
        if (targetAcc is null || amount is null)
        {
            HandleNotification(card.Owner, "economy-notification-transfer-failed-title", "economy-notification-transfer-failed-body", default);
            return;
        }
        amount = (int) amount; // max coding

        if (!TryComp<EconomyManagerComponent>(card.Comp.Station, out var economy))
        {
            HandleNotification(card.Owner, "economy-notification-transfer-failed-title", "economy-notification-transfer-failed-body", default);
            return;
        }

        if (!_economy.TransferBalance(economy, targetAcc.Value, card.Comp.AccountId, amount.Value))
        {
            HandleNotification(card.Owner, "economy-notification-transfer-failed-title", "economy-notification-transfer-failed-body", default);
            return;
        }

        HandleNotification(card.Owner, "economy-notification-transfer-title", "economy-notification-transfer-body", amount);
        UpdateUIForCard(card);
    }

    ///

    private void UpdateUIForCard(EntityUid cardUid)
    {
        // Find any PDA containing this card and update its UI
        var query = EntityQueryEnumerator<NanoBankCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (comp.Card != cardUid || cartridge.LoaderUid == null)
                continue;

            UpdateUI((uid, comp), cartridge.LoaderUid.Value);
        }
    }

    private void UpdateUI(Entity<NanoBankCartridgeComponent> ent, EntityUid loader)
    {
        int accountId = 0;
        int pin = 0;
        bool notificationsMuted = false;
        bool logged = false;
        float nextPayment = 0;
        float balance = 0;

        NanoBankCardComponent? card = default;

        if (ent.Comp.Card != null && TryComp<NanoBankCardComponent>(ent.Comp.Card, out card))
        {
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
