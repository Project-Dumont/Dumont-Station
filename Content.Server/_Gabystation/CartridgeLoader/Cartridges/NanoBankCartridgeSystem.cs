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
    /// Notificação pós pagamento
    /// Estamos sempre supondo que cada conta está logada num só card, isso é errado. Todos os cards logados deveriam receber notificação.

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

    private void OnTransference(Entity<EconomyManagerComponent> ent, ref AccountTransferenceCompleted args)
    {
        //! Isso provavelmente tem um alto custo computacional, mas eu não sei outro jeito de fazer isso.
        // TODO: Novo metodo, o id recebe a mensagem e verifica se está na conta bancaria, se sim, envia uma outra mensagem pra cá.
        Log.Debug("Payment 1");
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
                break;
            case TransferenceTypes.Pursache:
                break;
            case TransferenceTypes.Withdraw:
                break;
            case TransferenceTypes.Deposit:
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

    private void HandleTransfer(Entity<NanoBankCardComponent> card, int? targetAcc, int? amount)
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

        var nanoBankUid = container.ContainedEntities
            .Where(HasComp<NanoBankCartridgeComponent>) // Pode acontecer do PDA ter mais de um nanobank instalado?
            .First();

        if (!TryComp<NanoBankCartridgeComponent>(nanoBankUid, out var nanoBankComp))
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
