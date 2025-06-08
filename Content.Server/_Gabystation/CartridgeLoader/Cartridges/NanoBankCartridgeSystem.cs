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

namespace Content.Server._Gabystation.CartridgeLoader.Cartridges;

public sealed class NanoBankCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly EconomyManagerSystem _economy = default!;
    [Dependency] private readonly StationSystem _station = default!;
    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        //SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        SubscribeLocalEvent<NanoBankCartridgeComponent, AccountPaymentCompleted>(OnPayment);
    }

    private void OnPayment(Entity<NanoBankCartridgeComponent> ent, ref AccountPaymentCompleted args)
    {
        throw new NotImplementedException();
    }

    public void OnPayment(AccountPaymentCompleted args)
    {
        var ents = AllEntityQuery<NanoBankCardComponent>();
        while (ents.MoveNext(out var uid, out var comp))
        {
            var station = _station.GetOwningStation(uid);
            if (!comp.LoggedIn || comp.AccountId is 0 || comp.AccountPin is 0 || station is null)
                continue;

            if (!TryComp<EconomyManagerComponent>(station, out var economyComp))
                continue;

            if (!_economy.ValidateLogin(economyComp, comp.AccountId, comp.AccountPin))
                continue;

            HandleNotification(uid, "economy-notification-tittle", "economy-notification-body", args.Payment);
        }
    }

    /// <summary>
    ///     Gets the ID card entity associated with a PDA.
    /// </summary>
    /// <returns>True if a valid NanoBank card was found</returns>
    private bool GetCardEntity(EntityUid cardUid,
        out Entity<NanoBankCardComponent> card)
    {
        card = default;

        if (!TryComp<PdaComponent>(cardUid, out var pda) ||
            pda.ContainedId == null ||
            !TryComp<NanoBankCardComponent>(pda.ContainedId, out var idCard))
            return false;

        card = (pda.ContainedId.Value, idCard);
        return true;
    }

    private void HandleNotification(EntityUid uid, string tittleLoc, string bodyLoc, float? amount)
    {
        if (!GetCardEntity(uid, out var bankCard))
            return;

        string body;
        if (amount is not null)
            body = Loc.GetString(bodyLoc, ("amount", amount));
        else
            body = Loc.GetString(bodyLoc);

        _cartridge.SendNotification(uid,
            Loc.GetString(tittleLoc),
            body);
    }
}
