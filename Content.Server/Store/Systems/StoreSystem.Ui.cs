// <Trauma>
// Dumont start
using Content.Server.Mindshield;
using Content.Server._Goobstation.Wizard.Store;
using Content.Shared._Goobstation.Wizard.Refund;

using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using Content.Goobstation.Shared.ManifestListings;
// </Trauma>
using System.Linq;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared.Actions;
using Content.Shared.Database;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.NPC.Systems;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private IAdminLogManager _admin = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private ActionUpgradeSystem _actionUpgrade = default!;
    [Dependency] private NpcFactionSystem _npcFaction = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private StackSystem _stack = default!;
    // Dumont start
    [Dependency] private Content.Shared.Charges.Systems.SharedChargesSystem _charges = default!;
    // Dumont end
    [Dependency] private MindShieldSystem _mindShield = default!;

    private void InitializeUi()
    {
        SubscribeLocalEvent<StoreComponent, StoreRequestUpdateInterfaceMessage>(OnRequestUpdate);
        SubscribeLocalEvent<StoreComponent, StoreBuyListingMessage>(OnBuyRequest);
        SubscribeLocalEvent<StoreComponent, StoreRequestWithdrawMessage>(OnRequestWithdraw);
        SubscribeLocalEvent<StoreComponent, StoreRequestRefundMessage>(OnRequestRefund);
        SubscribeLocalEvent<StoreComponent, RefundEntityDeletedEvent>(OnRefundEntityDeleted);

        SubscribeLocalEvent<RemoteStoreComponent, StoreRequestUpdateInterfaceMessage>((e, c, ev) =>
            RemoteStoreRelay((e, c), ev));
        SubscribeLocalEvent<RemoteStoreComponent, StoreBuyListingMessage>((e, c, ev) =>
            RemoteStoreRelay((e, c), ev));
        SubscribeLocalEvent<RemoteStoreComponent, StoreRequestWithdrawMessage>((e, c, ev) =>
            RemoteStoreRelay((e, c), ev));
        SubscribeLocalEvent<RemoteStoreComponent, StoreRequestRefundMessage>((e, c, ev) =>
            RemoteStoreRelay((e, c), ev));
        SubscribeLocalEvent<RemoteStoreComponent, RefundEntityDeletedEvent>((e, c, ev) =>
            RemoteStoreRelay((e, c), ev));
    }

    private void OnRefundEntityDeleted(Entity<StoreComponent> ent, ref RefundEntityDeletedEvent args)
    {
        ent.Comp.BoughtEntities.Remove(args.Uid);
    }

    private void RemoteStoreRelay(Entity<RemoteStoreComponent> entity, object ev)
    {
        if (entity.Comp.Store == null || !TryComp<StoreComponent>(entity.Comp.Store, out var store))
            return;

        RaiseLocalEvent(entity.Comp.Store.Value, ev);
    }

    private void OnRequestUpdate(EntityUid uid, StoreComponent component, StoreRequestUpdateInterfaceMessage args)
    {
        UpdateUserInterface(args.Actor, GetEntity(args.Entity), component);
    }

    private void BeforeActivatableUiOpen(EntityUid uid, StoreComponent component, BeforeActivatableUIOpenEvent args)
    {
        UpdateUserInterface(args.User, uid, component);
    }

    /// <summary>
    /// Handles whenever a purchase was made.
    /// </summary>
    private void OnBuyRequest(EntityUid uid, StoreComponent component, StoreBuyListingMessage msg)
    {
        var listing = component.FullListingsCatalog.FirstOrDefault(x => x.ID.Equals(msg.Listing.Id));

        if (listing == null) //make sure this listing actually exists
        {
            Log.Debug("listing does not exist");
            return;
        }

        var buyer = msg.Actor;

        //verify that we can actually buy this listing and it wasn't added
        if (!ListingHasCategory(listing, component.Categories))
            return;

        //condition checking because why not
        if (listing.Conditions != null)
        {
            var args = new ListingConditionArgs(component.AccountOwner ?? GetBuyerMind(buyer), uid, listing, EntityManager);
            var conditionsMet = listing.Conditions.All(condition => condition.Condition(args));

            if (!conditionsMet)
                return;
        }

        // <Trauma>
        var cost = listing.TryGetSelectedCurrenciesForPurchase(component.Balance, out var skipped);
        if (skipped)
            cost = listing.Cost.ToDictionary();
        else if (cost == null)
            return;
        // </Trauma>

        //check that we have enough money
        // var cost = listing.Cost; // Goobstation
        foreach (var (currency, amount) in cost)
        {
            if (amount == FixedPoint2.Zero) // Trauma - skip balance check if listing costs 0
                continue;

            if (!component.Balance.TryGetValue(currency, out var balance) || balance < amount)
            {
                return;
            }
        }

        // <Trauma>
        OnPurchase(listing);
        if (Mind.TryGetMind(buyer, out var mindId, out _))
        {
            var ev = new ListingPurchasedEvent(buyer, uid, listing, cost);
            RaiseLocalEvent(mindId, ref ev);
        }
        // </Trauma>

        /* Trauma
        if (!IsOnStartingMap(uid, component))
            DisableRefund(uid, component);
        */

        //subtract the cash
        foreach (var (currency, amount) in cost)
        {
            if (amount > FixedPoint2.Zero) // Trauma - skip balance check if listing costs 0
                component.Balance[currency] -= amount;

            component.BalanceSpent.TryAdd(currency, FixedPoint2.Zero);

            component.BalanceSpent[currency] += amount;
        }

        // Dumont start
        var currencyEvent = new CurrencyUpdatedEvent(cost.ToDictionary(entry => entry.Key, entry => -entry.Value));
        RaiseLocalEvent(uid, currencyEvent);
        // Dumont end

        //apply components
        if (listing.ProductComponents != null)
        {
            if (ProtoMan.Resolve(listing.ProductComponents, out var productComponentsEntity))
                EntityManager.AddComponents(buyer, productComponentsEntity.Components);
        }

        //spawn entity
        if (listing.ProductEntity != null)
        {
            var product = Spawn(listing.ProductEntity, Transform(buyer).Coordinates);
            _hands.PickupOrDrop(buyer, product);

            RaiseLocalEvent(product, new ItemPurchasedEvent(buyer));

            HandleRefundComp(uid, component, product, cost, listing); // Trauma - added cost and listing

            var xForm = Transform(product);

            if (xForm.ChildCount > 0)
            {
                var childEnumerator = xForm.ChildEnumerator;
                while (childEnumerator.MoveNext(out var child))
                {
                    component.BoughtEntities.Add(child);
                }
            }
        }

        //give action
        if (!string.IsNullOrWhiteSpace(listing.ProductAction))
        {
            // Dumont start
            EntityUid? actionId = null;
            var actionOwner = buyer;
            var useMind = !listing.ApplyToMob && component.GrantActionsToMind && Mind.TryGetMind(buyer, out _, out _);
            if (useMind && Mind.TryGetMind(buyer, out var buyerMind, out _))
                actionOwner = buyerMind;

            if (listing.ProductActionCharges is > 0 &&
                TryComp<Content.Shared.Actions.Components.ActionsComponent>(actionOwner, out var ownedActions))
            {
                foreach (var existing in ownedActions.Actions)
                {
                    if (!Exists(existing) || MetaData(existing).EntityPrototype?.ID != listing.ProductAction.Value.Id)
                        continue;

                    _charges.AddCharges(existing, listing.ProductActionCharges.Value);
                    actionId = existing;
                    break;
                }
            }

            actionId ??= useMind
                ? _actionContainer.AddAction(actionOwner, listing.ProductAction)
                : _actions.AddAction(buyer, listing.ProductAction);
            // Dumont end

            // Add the newly bought action entity to the list of bought entities
            // And then add that action entity to the relevant product upgrade listing, if applicable
            if (actionId != null)
            {
                HandleRefundComp(uid, component, actionId.Value, cost, listing); // Trauma - added cost and listing

                if (listing.ProductUpgradeId != null)
                {
                    foreach (var upgradeListing in component.FullListingsCatalog)
                    {
                        if (upgradeListing.ID == listing.ProductUpgradeId)
                        {
                            upgradeListing.ProductActionEntity = actionId.Value;
                            break;
                        }
                    }
                }
            }
        }

        if (listing is { ProductUpgradeId: not null, ProductActionEntity: not null })
        {
            ListingDataWithCostModifiers? originalListing = null; // Goobstation
            var costCopy = cost.ToDictionary(); // Goobstation
            if (listing.ProductActionEntity != null)
            {
                if (TryComp(listing.ProductActionEntity.Value, out StoreRefundComponent? storeRefund)) // Goobstation
                {
                    foreach (var (key, value) in storeRefund.BalanceSpent)
                    {
                        costCopy.TryAdd(key, FixedPoint2.Zero);
                        costCopy[key] += value;
                    }
                    originalListing = storeRefund.Data;
                }
                component.BoughtEntities.Remove(listing.ProductActionEntity.Value);
            }

            if (!_actionUpgrade.TryUpgradeAction(listing.ProductActionEntity, out var upgradeActionId))
            {
                if (listing.ProductActionEntity != null)
                    HandleRefundComp(uid, component, listing.ProductActionEntity.Value, costCopy, originalListing, true); // Trauma - added costCopy, originalListing and true

                return;
            }

            listing.ProductActionEntity = upgradeActionId;

            if (upgradeActionId != null)
                HandleRefundComp(uid, component, upgradeActionId.Value, cost, originalListing, true); // Trauma - added cost, originalListing and true
        }

        if (listing.ProductEvent != null)
        {
            // <Trauma>
            if (listing.RaiseProductEventOnMind && mindId != EntityUid.Invalid)
                RaiseLocalEvent(mindId, listing.ProductEvent);
            else if (!listing.RaiseProductEventOnUser)
            // </Trauma>
                RaiseLocalEvent(listing.ProductEvent);
            else
                RaiseLocalEvent(buyer, listing.ProductEvent);
        }

        // <Trauma>
        /*
        if (listing.DisableRefund)
        {
            component.RefundAllowed = false;
        }
        */
        if (listing.BlockRefundListings.Count > 0)
        {
            foreach (var listingData in component.FullListingsCatalog.Where(x => listing.BlockRefundListings.Contains(x.ID)))
            {
                listingData.DisableRefund = true;
            }
        }

        listing.PurchaseCostHistory.Add(cost);
        // </Trauma>

        //log dat shit.
        var logImpact = LogImpact.Low;
        var logExtraInfo = "";
        if (component.ExpectedFaction?.Count > 0 && !_npcFaction.IsMemberOfAny(buyer, component.ExpectedFaction))
        {
            logImpact = LogImpact.High;
            logExtraInfo = ", but was not from an expected faction";

            var isMindshielded = HasComp<Content.Shared.Mindshield.Components.MindShieldComponent>(buyer);
            if (isMindshielded)
            {
                logImpact = LogImpact.Extreme;
                logExtraInfo += " while also possessing a mindshield";
            }
        }

        _admin.Add(LogType.StorePurchase,
            logImpact,
            $"{ToPrettyString(buyer):player} purchased listing \"{ListingLocalisationHelpers.GetLocalisedNameOrEntityName(listing, ProtoMan)}\" from {ToPrettyString(uid)}{logExtraInfo}.");

        listing.PurchaseAmount++; //track how many times something has been purchased
        // Dumont start
        if (listing.SaleLimit > 0 && listing.PurchaseAmount >= listing.SaleLimit)
            listing.RemoveCostModifier("DumontSales");
        // Dumont end
        if (msg.SoundSource != null && GetEntity(msg.SoundSource) != null)
            _audio.PlayEntity(component.BuySuccessSound, msg.Actor, GetEntity(msg.SoundSource.Value)); //cha-ching!

        var buyFinished = new StoreBuyFinishedEvent
        {
            PurchasedItem = listing,
            StoreUid = uid
        };
        RaiseLocalEvent(ref buyFinished);

        UpdateUserInterface(buyer, uid, component);
        UpdateRefundUserInterface(uid, component); // Goobstation
        if (listing.ResetRestockOnPurchase) // goobstation edit start
        {
            // making sure that you cant buy some stuff endlessly if they are not meant to
            var restockDuration = listing.RestockAfterPurchase ?? listing.RestockTime; // Just use the value directly.
            listing.RestockTime = _timing.CurTime.Subtract(_ticker.RoundStartTimeSpan) + restockDuration;
        } // goob edit end

    }

    /// <summary>
    /// Handles dispensing the currency you requested to be withdrawn.
    /// </summary>
    /// <remarks>
    /// This would need to be done should a currency with decimal values need to use it.
    /// not quite sure how to handle that
    /// </remarks>
    private void OnRequestWithdraw(EntityUid uid, StoreComponent component, StoreRequestWithdrawMessage msg)
    {
        if (msg.Amount <= 0)
            return;

        //make sure we have enough cash in the bank and we actually support this currency
        if (!component.Balance.TryGetValue(msg.Currency, out var currentAmount) || currentAmount < msg.Amount)
            return;

        //make sure a malicious client didn't send us random shit
        if (!ProtoMan.TryIndex<CurrencyPrototype>(msg.Currency, out var proto))
            return;

        //we need an actually valid entity to spawn. This check has been done earlier, but just in case.
        if (proto.Cash == null || !proto.CanWithdraw)
            return;

        var buyer = msg.Actor;

        FixedPoint2 amountRemaining = msg.Amount;
        var coordinates = Transform(buyer).Coordinates;

        var sortedCashValues = proto.Cash.Keys.OrderByDescending(x => x).ToList();
        foreach (var value in sortedCashValues)
        {
            var cashId = proto.Cash[value];
            var amountToSpawn = (int) MathF.Floor((float) (amountRemaining / value));
            var ents = _stack.SpawnMultiple(cashId, amountToSpawn, coordinates);
            if (ents.FirstOrDefault() is {} ent)
                _hands.PickupOrDrop(buyer, ent);
            amountRemaining -= value * amountToSpawn;
        }

        component.Balance[msg.Currency] -= msg.Amount;
        UpdateUserInterface(buyer, uid, component);
    }

    private void OnRequestRefund(EntityUid uid, StoreComponent component, StoreRequestRefundMessage args)
    {
        // TODO: Remove guardian/holopara

        if (args.Actor is not { Valid: true } buyer)
            return;

        // Goob edit start
        if (!UI.HasUi(uid, RefundUiKey.Key))
            component.RefundAllowed = false;

        if (!component.RefundAllowed)
            UI.CloseUi(uid, RefundUiKey.Key);

        if (!UI.IsUiOpen(uid, RefundUiKey.Key, buyer))
            UI.OpenUi(uid, RefundUiKey.Key, buyer);
        else
        {
            UI.CloseUi(uid, RefundUiKey.Key, buyer);
            return;
        }

        UpdateRefundUserInterface(uid, component);

        /* if (!IsOnStartingMap(uid, component))
        {
            DisableRefund(uid, component);
            UpdateUserInterface(buyer, uid, component);
        }

        if (!component.RefundAllowed || component.BoughtEntities.Count == 0)
            return;

        _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded their purchases from {ToPrettyString(uid):store}");

        for (var i = component.BoughtEntities.Count - 1; i >= 0; i--)
        {
            var purchase = component.BoughtEntities[i];

            if (!Exists(purchase))
                continue;

            component.BoughtEntities.RemoveAt(i);

            _actionContainer.RemoveAction(purchase, logMissing: false);

            Del(purchase);
        }

        component.BoughtEntities.Clear();

        foreach (var (currency, value) in component.BalanceSpent)
        {
            component.Balance[currency] += value;
        }

        // Reset store back to its original state
        RefreshAllListings(component);
        component.BalanceSpent = new();
        UpdateUserInterface(buyer, uid, component); */

        // Goob edit end
    }

    // Goobstation start
    public void UpdateRefundUserInterface(EntityUid uid, StoreComponent component)
    {
        if (!IsOnStartingMap(uid, component))
            UI.SetUiState(uid, RefundUiKey.Key, new StoreRefundState(new(), true));
        else
        {
            List<RefundListingData> listings = new();
            foreach (var bought in component.BoughtEntities)
            {
                if (!Exists(bought) || !TryComp(bought, out StoreRefundComponent? refundComp) ||
                    refundComp.Data == null || refundComp.StoreEntity != uid || refundComp.Data.DisableRefund)
                    continue;

                var name = ListingLocalisationHelpers.GetLocalisedNameOrEntityName(refundComp.Data, ProtoMan);
                listings.Add(new RefundListingData(GetNetEntity(bought), name));
            }

            UI.SetUiState(uid, RefundUiKey.Key, new StoreRefundState(listings, false));
        }
    }

    public bool RefundListing(EntityUid uid, StoreComponent component, EntityUid boughtEntity, EntityUid buyer, bool log)
    {
        if (!IsOnStartingMap(uid, component) || !Exists(boughtEntity) ||
            !TryComp(boughtEntity, out StoreRefundComponent? refundComp) || refundComp.Data == null ||
            refundComp.StoreEntity != uid || refundComp.Data.DisableRefund)
            return false;

        if (log)
            _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded {ToPrettyString(boughtEntity):purchase} from {ToPrettyString(uid):store}");

        foreach (var (currency, value) in refundComp.BalanceSpent)
        {
            component.Balance.TryAdd(currency, FixedPoint2.Zero);
            component.Balance[currency] += value;

            if (component.BalanceSpent.ContainsKey(currency))
                component.BalanceSpent[currency] -= value;
        }

        if (refundComp.Data.ProductUpgradeId != null)
        {
            foreach (var upgradeListing in component.FullListingsCatalog.Where(upgradeListing =>
                         upgradeListing.ID == refundComp.Data.ProductUpgradeId))
            {
                upgradeListing.PurchaseAmount = 0;
                upgradeListing.PurchaseCostHistory.Clear();
                break;
            }
        }

        component.BoughtEntities.Remove(boughtEntity);

        if (_actions.GetAction(boughtEntity, false) is { } action)
            _actionContainer.RemoveAction((boughtEntity, action.Comp));

        var listing = refundComp.Data;
        listing.PurchaseAmount = Math.Max(0, listing.PurchaseAmount - 1);
        listing.PurchaseCostHistory = listing.PurchaseCostHistory.Take(listing.PurchaseAmount).ToList();

        Del(boughtEntity);

        return true;
    }

    public static void DisableListingRefund(ListingData? data)
    {
        if (data != null)
            data.DisableRefund = true;
    }
    // Goobstation end

    private void HandleRefundComp(EntityUid uid,
        StoreComponent component,
        EntityUid purchase,
        IReadOnlyDictionary<ProtoId<CurrencyPrototype>, FixedPoint2> cost,
        ListingDataWithCostModifiers? data,
        bool overrideCost = false) // Trauma - added cost, data and overrideCost
    {
        component.BoughtEntities.Add(purchase);
        var refundComp = EnsureComp<StoreRefundComponent>(purchase);
        refundComp.StoreEntity = uid;
        // Goobstation start
        if (overrideCost)
            refundComp.BalanceSpent = cost.ToDictionary();
        else
        {
            foreach (var (key, value) in cost)
            {
                refundComp.BalanceSpent.TryAdd(key, FixedPoint2.Zero);
                refundComp.BalanceSpent[key] += value;
            }
        }

        if (data != null)
            refundComp.Data = data;
        // Goobstation end

        refundComp.BoughtTime = _timing.CurTime;
    }

    public bool IsOnStartingMap(EntityUid store, StoreComponent component)
    {
        var xform = Transform(store);
        return component.StartingMap == xform.MapUid;
    }

    /// <summary>
    ///     Disables refunds for this store
    /// </summary>
    public void DisableRefund(EntityUid store, StoreComponent? component = null)
    {
        if (!Resolve(store, ref component))
            return;

        component.RefundAllowed = false;
    }
}

/// <summary>
/// Event of successfully finishing purchase in store (<see cref="StoreSystem"/>.
/// </summary>
/// <param name="StoreUid">EntityUid on which store is placed.</param>
/// <param name="PurchasedItem">ListingItem that was purchased.</param>
[ByRefEvent]
public readonly record struct StoreBuyFinishedEvent(
    EntityUid StoreUid,
    ListingDataWithCostModifiers PurchasedItem
);
