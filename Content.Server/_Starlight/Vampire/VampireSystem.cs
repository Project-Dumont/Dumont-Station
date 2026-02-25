// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 Dreykor <arguemeu@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Atmos.Rotting;
using Content.Server.Beam;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Server.Mind;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Prayer;
using Content.Shared.Stealth.Components;
using Content.Shared.Stunnable;
using Content.Shared.Vampire;
using Content.Shared.Charges.Systems;
using Content.Shared.Vampire.Components;
using Content.Shared.Charges.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Shared.Actions.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Server.Vampire;

public sealed partial class VampireSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly IngestionSystem _ingestion = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly RottingSystem _rotting = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MetabolizerSystem _metabolism = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedVampireSystem _vampire = default!;
    [Dependency] private readonly SharedChargesSystem _sharedChargesSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private Dictionary<string, EntityUid> _actionEntities = new();

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<VampireComponent, VampireSelfPowerEvent>(OnUseSelfPower);
        //SubscribeLocalEvent<VampireComponent, VampireTargetedPowerEvent>(OnUseTargetedPower);
        SubscribeLocalEvent<VampireComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<VampireComponent, VampireBloodChangedEvent>(OnVampireBloodChangedEvent);
        SubscribeLocalEvent<VampireComponent, ComponentGetState>(GetState);
        SubscribeLocalEvent<VampireComponent, VampireMutationPrototypeSelectedMessage>(OnMutationSelected);
        SubscribeLocalEvent<VampireComponent, ComponentInit>(OnVampireComponentInit);

        InitializePowers();
        InitializeObjectives();
    }

    /// <summary>
    /// Handles healing, stealth and damaging in space
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var stealthQuery = EntityQueryEnumerator<VampireComponent, VampireStealthComponent>();
        while (stealthQuery.MoveNext(out var uid, out var vampire, out var stealth))
        {
            if (vampire == null || stealth == null)
                continue;
            if (stealth.NextStealthTick <= 0)
            {
                stealth.NextStealthTick = 1;
                if (!SubtractBloodEssence((uid, vampire), stealth.Upkeep) || _vampire.GetBloodEssence(uid) < FixedPoint2.New(300))
                {
                    RemCompDeferred<StealthOnMoveComponent>(uid);
                    RemCompDeferred<StealthComponent>(uid);
                    RemCompDeferred<VampireStealthComponent>(uid);
                    _popup.PopupEntity(Loc.GetString("vampire-cloak-disable"), uid, uid);
                    if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(300))
                    {
                        var vampireUid = new Entity<VampireComponent>(uid, vampire);
                        var ev = new VampireBloodChangedEvent();
                        RaiseLocalEvent(vampireUid, ev);
                    }
                }
            }
            stealth.NextStealthTick -= frameTime;
        }

        var healingQuery = EntityQueryEnumerator<VampireComponent, VampireHealingComponent>();
        while (healingQuery.MoveNext(out var uid, out _, out var healing))
        {
            if (healing == null)
                continue;

            if (healing.NextHealTick <= 0)
            {
                healing.NextHealTick = 1;
                DoCoffinHeal(uid, healing);
            }
            healing.NextHealTick -= frameTime;
        }

        var spaceQuery = EntityQueryEnumerator<VampireComponent, VampireSpaceDamageComponent, DamageableComponent>();
        while (spaceQuery.MoveNext(out var uid, out var vampire, out var spacedamage, out var damage))
        {
            if (vampire == null || spacedamage == null)
                continue;

            if (IsInSpace(uid))
            {
                if (spacedamage.NextSpaceDamageTick <= 0)
                {
                    spacedamage.NextSpaceDamageTick = 1;
                    if (!SubtractBloodEssence((uid, vampire), FixedPoint2.New(1)))
                        DoSpaceDamage(uid, vampire, damage);
                }
                spacedamage.NextSpaceDamageTick -= frameTime;
            }
        }
    }

    private void OnExamined(EntityUid uid, VampireComponent component, ExaminedEvent args)
    {
        if (HasComp<VampireFangsExtendedComponent>(uid) && args.IsInDetailsRange && !_ingestion.HasMouthAvailable(uid, uid))
            args.AddMarkup($"{Loc.GetString("vampire-fangs-extended-examine")}{Environment.NewLine}");
    }

    private bool AddBloodEssence(Entity<VampireComponent> vampire, FixedPoint2 quantity)
    {
        if (quantity < 0)
            return false;

        vampire.Comp.TotalBloodDrank += quantity.Float();
        vampire.Comp.Balance[VampireComponent.CurrencyProto] += quantity;

        UpdateBloodDisplay(vampire);

        var ev = new VampireBloodChangedEvent();
        RaiseLocalEvent(vampire, ev);

        return true;
    }

    private bool SubtractBloodEssence(Entity<VampireComponent> vampire, FixedPoint2 quantity)
    {
        if (quantity < 0)
            return false;

        if (vampire.Comp.Balance[VampireComponent.CurrencyProto] < quantity)
            return false;

        vampire.Comp.Balance[VampireComponent.CurrencyProto] -= quantity;

        UpdateBloodDisplay(vampire);

        var ev = new VampireBloodChangedEvent();
        RaiseLocalEvent(vampire, ev);

        return true;
    }

    /// <summary>
    /// Use the charges display on SummonHeirloom to show the remaining blood essence
    /// </summary>
    /// <param name="vampire"></param>
    public void UpdateBloodDisplay(EntityUid vampire)
    {
        if (!TryComp<VampireComponent>(vampire, out var comp))
            return;

        //Sanity check, you never know who is going to touch this code
        if (!comp.Balance.TryGetValue(VampireComponent.CurrencyProto, out var balance))
            return;

        var chargeDisplay = (int) Math.Round((decimal) balance);
        var mutationsAction = GetPowerEntity(comp, VampireComponent.MutationsActionPrototype);

        if (mutationsAction == null)
            return;

        if (TryComp<LimitedChargesComponent>(mutationsAction, out var chargesComp))
            _sharedChargesSystem.SetCharges(new Entity<LimitedChargesComponent?>(mutationsAction.Value, chargesComp), chargeDisplay);
    }

    private void OnVampireBloodChangedEvent(EntityUid uid, VampireComponent component, VampireBloodChangedEvent args)
    {
        if (TryComp<VampireAlertComponent>(uid, out var alertComp))
            _vampire.SetAlertBloodAmount(alertComp, _vampire.GetBloodEssence(uid).Int());

        EntityUid? newEntity = null;
        EntityUid entity = default;
        // Mutations
        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(50) && !_actionEntities.TryGetValue(VampireComponent.MutationsActionPrototype, out entity) && !HasComp<VampireStealthComponent>(uid))
        {
            _action.AddAction(uid, ref newEntity, VampireComponent.MutationsActionPrototype);
            if (newEntity != null)
                _actionEntities[VampireComponent.MutationsActionPrototype] = newEntity.Value;
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(50) && _actionEntities.TryGetValue(VampireComponent.MutationsActionPrototype, out entity) || HasComp<VampireStealthComponent>(uid))
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove(VampireComponent.MutationsActionPrototype);
        }

        //Hemomancer

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(200) && !_actionEntities.TryGetValue("ActionVampireBloodSteal", out entity) && component.CurrentMutation == VampireMutationsType.Hemomancer)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireBloodSteal");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireBloodSteal"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("BloodSteal"))
                {
                    component.UnlockedPowers.Add("BloodSteal", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(200) && _actionEntities.TryGetValue("ActionVampireBloodSteal", out entity) || component.CurrentMutation != VampireMutationsType.Hemomancer)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireBloodSteal");
        }

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(300) && !_actionEntities.TryGetValue("ActionVampireScreech", out entity) && component.CurrentMutation == VampireMutationsType.Hemomancer)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireScreech");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireScreech"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("Screech"))
                {
                    component.UnlockedPowers.Add("Screech", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(300) && _actionEntities.TryGetValue("ActionVampireScreech", out entity) || component.CurrentMutation != VampireMutationsType.Hemomancer)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireScreech");
        }

        //Umbrae

        if (_actionEntities.TryGetValue("ActionVampireCloakOfDarkness", out entity) && !HasComp<VampireStealthComponent>(uid) && _vampire.GetBloodEssence(uid) < FixedPoint2.New(300))
            _actionEntities.Remove("ActionVampireCloakOfDarkness");

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(200) && !_actionEntities.TryGetValue("ActionVampireGlare", out entity) && component.CurrentMutation == VampireMutationsType.Umbrae)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireGlare");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireGlare"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("Glare"))
                {
                    component.UnlockedPowers.Add("Glare", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(200) && _actionEntities.TryGetValue("ActionVampireGlare", out entity) || component.CurrentMutation != VampireMutationsType.Umbrae)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireGlare");
        }

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(300) && !_actionEntities.TryGetValue("ActionVampireCloakOfDarkness", out entity) && component.CurrentMutation == VampireMutationsType.Umbrae)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireCloakOfDarkness");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireCloakOfDarkness"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("CloakOfDarkness"))
                {
                    component.UnlockedPowers.Add("CloakOfDarkness", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(300) && _actionEntities.TryGetValue("ActionVampireCloakOfDarkness", out entity) || component.CurrentMutation != VampireMutationsType.Umbrae)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireCloakOfDarkness");
        }

        //Gargantua

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(200) && !_actionEntities.TryGetValue("ActionVampireUnnaturalStrength", out entity) && component.CurrentMutation == VampireMutationsType.Gargantua)
        {
            var vampire = new Entity<VampireComponent>(uid, component);

            UnnaturalStrength(vampire);

            _actionEntities["ActionVampireUnnaturalStrength"] = vampire;
        }

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(300) && !_actionEntities.TryGetValue("ActionVampireSupernaturalStrength", out entity) && component.CurrentMutation == VampireMutationsType.Gargantua)
        {
            var vampire = new Entity<VampireComponent>(uid, component);

            SupernaturalStrength(vampire);

            _actionEntities["ActionVampireSupernaturalStrength"] = vampire;
        }

        //Bestia

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(200) && !_actionEntities.TryGetValue("ActionVampireBatform", out entity) && component.CurrentMutation == VampireMutationsType.Bestia)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireBatform");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireBatform"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("PolymorphBat"))
                {
                    component.UnlockedPowers.Add("PolymorphBat", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(200) && _actionEntities.TryGetValue("ActionVampireBatform", out entity) || component.CurrentMutation != VampireMutationsType.Bestia)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireBatform");
        }

        if (_vampire.GetBloodEssence(uid) >= FixedPoint2.New(300) && !_actionEntities.TryGetValue("ActionVampireMouseform", out entity) && component.CurrentMutation == VampireMutationsType.Bestia)
        {
            _action.AddAction(uid, ref newEntity, "ActionVampireMouseform");
            if (newEntity != null)
            {
                _actionEntities["ActionVampireMouseform"] = newEntity.Value;
                if (!component.UnlockedPowers.ContainsKey("PolymorphMouse"))
                {
                    component.UnlockedPowers.Add("PolymorphMouse", newEntity);
                }
            }
        }
        else if (_vampire.GetBloodEssence(uid) < FixedPoint2.New(300) && _actionEntities.TryGetValue("ActionVampireMouseform", out entity) || component.CurrentMutation != VampireMutationsType.Bestia)
        {
            if (!TryComp<ActionsComponent>(uid, out var comp))
                return;

            _action.RemoveAction((uid, comp), entity);
            _actionContainer.RemoveAction(entity);
            _actionEntities.Remove("ActionVampireMouseform");
        }
    }

    private void DoSpaceDamage(EntityUid uid, VampireComponent comp, DamageableComponent damage)
    {
        var damageSpec = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Heat"), 2.5);
        _damageableSystem.TryChangeDamage(uid, damageSpec, true, false, damage, uid);
        _popup.PopupEntity(Loc.GetString("vampire-startlight-burning"), uid, uid, PopupType.LargeCaution);
    }

    private bool IsInSpace(EntityUid vampireUid)
    {
        var vampireTransform = Transform(vampireUid);
        var vampirePosition = _transform.GetMapCoordinates(vampireTransform);

        if (!_mapMan.TryFindGridAt(vampirePosition, out _, out var grid))
            return true;

        if (!_mapSystem.TryGetTileRef(vampireUid, grid, vampireTransform.Coordinates, out var tileRef))
            return true;

        return tileRef.Tile.IsEmpty
            || _turf.IsSpace(tileRef)
            || _turf.GetContentTileDefinition(tileRef.Tile).ID == "Lattice";
    }

    private bool IsNearPrayable(EntityUid vampireUid)
    {
        var mapCoords = _transform.GetMapCoordinates(vampireUid);

        var nearbyPrayables = _entityLookup.GetEntitiesInRange<PrayableComponent>(mapCoords, 5);
        foreach (var prayable in nearbyPrayables)
        {
            if (Transform(prayable).Anchored)
                return true;
        }

        return false;
    }

    private void OnMutationSelected(EntityUid uid, VampireComponent component, VampireMutationPrototypeSelectedMessage args)
    {
        if (component.CurrentMutation == args.SelectedId)
            return;
        ChangeMutation(uid, args.SelectedId, component);
    }

    private void ChangeMutation(EntityUid uid, VampireMutationsType newMutation, VampireComponent component)
    {
        var vampire = new Entity<VampireComponent>(uid, component);
        if (SubtractBloodEssence(vampire, FixedPoint2.New(50)))
        {
            component.CurrentMutation = newMutation;
            UpdateUi(uid, component);
            var ev = new VampireBloodChangedEvent();
            RaiseLocalEvent(uid, ev);
            TryOpenUi(uid, component.Owner, component);
        }
    }

    private void GetState(EntityUid uid, VampireComponent component, ref ComponentGetState args)
    {
        args.State = new VampireMutationComponentState
        {
            SelectedMutation = component.CurrentMutation
        };
    }

    private void TryOpenUi(EntityUid uid, EntityUid user, VampireComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        if (!TryComp(user, out ActorComponent? actor))
            return;
        _uiSystem.TryToggleUi(uid, VampireMutationUiKey.Key, actor.PlayerSession);
    }

    public void UpdateUi(EntityUid uid, VampireComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        var state = new VampireMutationBoundUserInterfaceState(component.VampireMutations, component.CurrentMutation);
        _uiSystem.SetUiState(uid, VampireMutationUiKey.Key, state);
    }

    private void OnVampireComponentInit(EntityUid uid, VampireComponent component, ComponentInit args)
    {
        // Garante que o vampiro tenha CollectiveMindComponent com canal VampireMind
        var mindComp = EnsureComp<CollectiveMindComponent>(uid);
        if (mindComp.DefaultChannel != "VampireMind")
            mindComp.DefaultChannel = "VampireMind";
        if (!mindComp.Channels.Contains("VampireMind"))
            mindComp.Channels.Add("VampireMind");
    }
}
