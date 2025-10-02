// SPDX-FileCopyrightText: 2022 Andreas Kämper <andreas@kaemper.tech>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Emag.Components;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Advertise.Components;
using Content.Shared.Advertise.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.VendingMachines;

public abstract partial class SharedVendingMachineSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private   readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private   readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedPointLightSystem Light = default!;
    [Dependency] private   readonly SharedPowerReceiverSystem _receiver = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UISystem = default!;
    [Dependency] protected readonly IRobustRandom Randomizer = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VendingMachineComponent, ComponentGetState>(OnVendingGetState);
        SubscribeLocalEvent<VendingMachineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VendingMachineComponent, GotEmaggedEvent>(OnEmagged);

        SubscribeLocalEvent<VendingMachineRestockComponent, AfterInteractEvent>(OnAfterInteract);

        // GabyStation -> Economy,Unpredict VendingMachine
        // Subs.BuiEvents<VendingMachineComponent>, OnInventoryEjectMessage, OnMapInit, IsAuthorized
        // TryEjectVendorItem, Deny, AuthorizedVend, RestockInventoryFromPrototype, AddInventoryFromPrototype
        // All moved to Server.
    }

    private void OnVendingGetState(Entity<VendingMachineComponent> entity, ref ComponentGetState args)
    {
        var component = entity.Comp;

        var inventory = new Dictionary<string, VendingMachineInventoryEntry>();
        var emaggedInventory = new Dictionary<string, VendingMachineInventoryEntry>();
        var contrabandInventory = new Dictionary<string, VendingMachineInventoryEntry>();

        foreach (var weh in component.Inventory)
        {
            inventory[weh.Key] = new(weh.Value);
        }

        foreach (var weh in component.EmaggedInventory)
        {
            emaggedInventory[weh.Key] = new(weh.Value);
        }

        foreach (var weh in component.ContrabandInventory)
        {
            contrabandInventory[weh.Key] = new(weh.Value);
        }

        args.State = new VendingMachineComponentState()
        {
            Inventory = inventory,
            EmaggedInventory = emaggedInventory,
            ContrabandInventory = contrabandInventory,
            Contraband = component.Contraband,
            EjectEnd = component.EjectEnd,
            DenyEnd = component.DenyEnd,
            DispenseOnHitEnd = component.DispenseOnHitEnd,
        };
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VendingMachineComponent>();
        var curTime = Timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Ejecting)
            {
                if (curTime > comp.EjectEnd)
                {
                    comp.EjectEnd = null;
                    Dirty(uid, comp);

                    EjectItem(uid, comp);
                    UpdateUI((uid, comp));
                }
            }

            if (comp.Denying)
            {
                if (curTime > comp.DenyEnd)
                {
                    comp.DenyEnd = null;
                    Dirty(uid, comp);

                    TryUpdateVisualState((uid, comp));
                }
            }

            if (comp.DispenseOnHitCoolingDown)
            {
                if (curTime > comp.DispenseOnHitEnd)
                {
                    comp.DispenseOnHitEnd = null;
                    Dirty(uid, comp);
                }
            }
        }
    }

    protected virtual void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args) { }

    protected virtual void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false) { }

    protected VendingMachineInventoryEntry? GetEntry(EntityUid uid, string entryId, InventoryType type, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return null;

        if (type == InventoryType.Emagged && HasComp<EmaggedComponent>(uid))
            return component.EmaggedInventory.GetValueOrDefault(entryId);

        if (type == InventoryType.Contraband && component.Contraband)
            return component.ContrabandInventory.GetValueOrDefault(entryId);

        return component.Inventory.GetValueOrDefault(entryId);
    }

    protected virtual void UpdateUI(Entity<VendingMachineComponent?> entity) { }

    /// <summary>
    /// Tries to update the visuals of the component based on its current state.
    /// </summary>
    public void TryUpdateVisualState(Entity<VendingMachineComponent?> entity)
    {
        if (!Resolve(entity.Owner, ref entity.Comp))
            return;

        var finalState = VendingMachineVisualState.Normal;
        if (entity.Comp.Broken)
        {
            finalState = VendingMachineVisualState.Broken;
        }
        else if (entity.Comp.Ejecting)
        {
            finalState = VendingMachineVisualState.Eject;
        }
        else if (entity.Comp.Denying)
        {
            finalState = VendingMachineVisualState.Deny;
        }
        else if (!_receiver.IsPowered(entity.Owner))
        {
            finalState = VendingMachineVisualState.Off;
        }

        // TODO: You know this should really live on the client with netsync off because client knows the state.
        if (Light.TryGetLight(entity.Owner, out var pointlight))
        {
            var lightEnabled = finalState != VendingMachineVisualState.Broken && finalState != VendingMachineVisualState.Off;
            Light.SetEnabled(entity.Owner, lightEnabled, pointlight);
        }

        _appearanceSystem.SetData(entity.Owner, VendingMachineVisuals.VisualState, finalState);
    }

    // Gaby change - now in server
    /*public void RestockInventoryFromPrototype(EntityUid uid,
        VendingMachineComponent? component = null, float restockQuality = 1f) { }*/

    private void OnEmagged(EntityUid uid, VendingMachineComponent component, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        // only emag if there are emag-only items
        args.Handled = component.EmaggedInventory.Count > 0;
    }

    /// <summary>
    /// Returns all of the vending machine's inventory. Only includes emagged and contraband inventories if
    /// <see cref="EmaggedComponent"/> with the EmagType.Interaction flag exists and <see cref="VendingMachineComponent.Contraband"/> is true
    /// are <c>true</c> respectively.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public List<VendingMachineInventoryEntry> GetAllInventory(EntityUid uid, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new();

        var inventory = new List<VendingMachineInventoryEntry>(component.Inventory.Values);

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            inventory.AddRange(component.EmaggedInventory.Values);

        if (component.Contraband)
            inventory.AddRange(component.ContrabandInventory.Values);

        return inventory;
    }

    public List<VendingMachineInventoryEntry> GetAvailableInventory(EntityUid uid, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return new();

        return GetAllInventory(uid, component).Where(_ => _.Amount > 0).ToList();
    }
}
