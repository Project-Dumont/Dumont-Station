// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Anomaly.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Materials;
using Content.Server.Pinpointer;
using Content.Server.Power.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Research.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Prototypes;
using Content.Shared.Construction;
using Content.Shared.Materials;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Research.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Anomaly;

public sealed partial class AdvancedAnomalyGeneratorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private ResearchSystem _research = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedIdCardSystem _idCard = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedMaterialStorageSystem _material = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, MaterialAmountChangedEvent>(OnRefresh);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, ResearchServerPointsChangedEvent>(OnRefresh);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, ResearchRegistrationChangedEvent>(OnRefresh);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, AdvancedAnomalyGeneratorGenerateMessage>(OnGenerate);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, MachineDeconstructedEvent>(OnDeconstructed, before: [typeof(MaterialStorageSystem)]);
        SubscribeLocalEvent<GeneratingAdvancedAnomalyGeneratorComponent, ComponentShutdown>((_, comp, _) => _audio.Stop(comp.AudioStream));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneratingAdvancedAnomalyGeneratorComponent, AdvancedAnomalyGeneratorComponent>();
        while (query.MoveNext(out var uid, out var generating, out var comp))
        {
            if (_timing.CurTime >= generating.EndTime)
                FinishGeneration((uid, comp), generating);
        }
    }

    private void OnRefresh<T>(Entity<AdvancedAnomalyGeneratorComponent> ent, ref T args) => UpdateUi(ent);

    private void OnUiOpened(EntityUid uid, AdvancedAnomalyGeneratorComponent comp, BoundUIOpenedEvent args)
    {
        // Wires and research use different UIs on the same machine, so they dont really need to care about our state
        // Same machine, different roommates, They can mind their own business
        if (args.UiKey is AdvancedAnomalyGeneratorUiKey)
            UpdateUi((uid, comp));
    }

    private void OnGenerate(EntityUid uid, AdvancedAnomalyGeneratorComponent comp, AdvancedAnomalyGeneratorGenerateMessage args)
    {
        if (!HasComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid)
            && StartGeneration((uid, comp), args.Entry, args.Tile, args.Actor) is { } error)
            Report((uid, comp), args.Actor, error);
    }

    private string? StartGeneration(
        Entity<AdvancedAnomalyGeneratorComponent> ent,
        ProtoId<AdvancedAnomalyGenerationPrototype> entryId,
        Vector2i tile,
        EntityUid user)
    {
        if (!this.IsPowered(ent, EntityManager))
            return Loc.GetString("advanced-anomaly-generator-error-unpowered");

        var remaining = ent.Comp.CooldownEnd - _timing.CurTime;
        if (remaining > TimeSpan.Zero)
            return Loc.GetString("advanced-anomaly-generator-error-cooldown", ("time", AdvancedAnomalyGeneratorRules.FormatCooldown(remaining)));

        if (!ent.Comp.AllowedAnomalies.Contains(entryId) || !_prototype.TryIndex(entryId, out var entry))
            return Loc.GetString("advanced-anomaly-generator-error-invalid-anomaly");

        if (!_research.TryGetClientServer(ent, out var server, out var serverComp))
            return Loc.GetString("advanced-anomaly-generator-error-no-server");

        if (serverComp.Points < entry.ResearchCost)
            return Loc.GetString("advanced-anomaly-generator-error-research", ("needed", entry.ResearchCost), ("available", serverComp.Points));

        if (GetTileError(ent, tile, out _) is { } tileError)
            return tileError;

        // We spend the points right away so two requests cant use the same ones, If it fails, FinishGeneration gives them back
        // No double spending here :D
        var material = _material.GetMaterialAmount(ent, ent.Comp.RequiredMaterial);
        if (!_material.TryChangeMaterialAmount(ent, ent.Comp.RequiredMaterial, -entry.MaterialCost))
        {
            return Loc.GetString("advanced-anomaly-generator-error-material",
                ("material", Loc.GetString(_prototype.Index(ent.Comp.RequiredMaterial).Name)),
                ("needed", entry.MaterialCost),
                ("available", material));
        }

        _research.ModifyServerPoints(server.Value, -entry.ResearchCost, serverComp);

        var generating = AddComp<GeneratingAdvancedAnomalyGeneratorComponent>(ent);
        generating.EndTime = _timing.CurTime + ent.Comp.GenerationLength;
        generating.Entry = entryId;
        generating.Tile = tile;
        generating.User = user;
        generating.Server = server.Value;
        generating.AudioStream = _audio.PlayPvs(ent.Comp.GeneratingSound, ent, AudioParams.Default.WithLoop(true))?.Entity;

        _appearance.SetData(ent, AnomalyGeneratorVisuals.Generating, true);
        UpdateUi(ent);
        return null;
    }

    private void OnDeconstructed(EntityUid uid, AdvancedAnomalyGeneratorComponent comp, MachineDeconstructedEvent args)
    {
        if (TryComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid, out var generating))
            Refund((uid, comp), generating);
    }

    private void Refund(Entity<AdvancedAnomalyGeneratorComponent> ent, GeneratingAdvancedAnomalyGeneratorComponent generating)
    {
        var entry = _prototype.Index(generating.Entry);
        _material.TryChangeMaterialAmount(ent, ent.Comp.RequiredMaterial, entry.MaterialCost);

        if (TryComp<ResearchServerComponent>(generating.Server, out var server))
            _research.ModifyServerPoints(generating.Server, entry.ResearchCost, server);
    }

    private void FinishGeneration(Entity<AdvancedAnomalyGeneratorComponent> ent, GeneratingAdvancedAnomalyGeneratorComponent generating)
    {
        RemComp<GeneratingAdvancedAnomalyGeneratorComponent>(ent);
        _appearance.SetData(ent, AnomalyGeneratorVisuals.Generating, false);

        var entry = _prototype.Index(generating.Entry);
        var user = Exists(generating.User) ? generating.User : null;

        // Tile got yoinked mid-generation, so at least give the poor guy his points back
        if (GetTileError(ent, generating.Tile, out var coords) is { } error)
        {
            Refund(ent, generating);
            Report(ent, user, error);
            return;
        }

        // The cooldown only starts if the generation actually works
        // No success, no cooldown, Nice try though
        ent.Comp.CooldownEnd = _timing.CurTime + ent.Comp.Cooldown;
        var anomaly = Spawn(entry.AnomalyPrototype, coords);
        var anomalyName = Loc.GetString(entry.Name);
        _audio.PlayPvs(ent.Comp.GeneratingFinishedSound, ent);

        Announce(ent, anomaly, anomalyName, user);
        Report(ent, user, Loc.GetString("advanced-anomaly-generator-success",
            ("anomaly", anomalyName), ("x", generating.Tile.X), ("y", generating.Tile.Y)));
    }

    private string? GetTileError(Entity<AdvancedAnomalyGeneratorComponent> ent, Vector2i tile, out EntityCoordinates coords)
    {
        coords = default;
        var uid = ent.Owner;
        var xform = Transform(uid);

        if (_station.GetOwningStation(uid, xform) is not { } station)
            return Loc.GetString("advanced-anomaly-generator-error-no-station");

        if (xform.GridUid is not { } grid || grid != _station.GetLargestGrid(station) || !TryComp<MapGridComponent>(grid, out var gridComp))
            return Loc.GetString("advanced-anomaly-generator-error-wrong-grid");

        if (!AdvancedAnomalyGeneratorRules.InRange(_map.TileIndicesFor(grid, gridComp, xform.Coordinates), tile, ent.Comp.Range))
            return Loc.GetString("advanced-anomaly-generator-error-out-of-range", ("range", ent.Comp.Range));

        if (!_map.TryGetTileRef(grid, gridComp, tile, out var tileRef)
            || tileRef.Tile.IsEmpty
            || _atmosphere.IsTileSpace(grid, xform.MapUid, tile)
            || _atmosphere.IsTileAirBlocked(grid, tile, mapGridComp: gridComp))
            return Loc.GetString("advanced-anomaly-generator-error-invalid-location");

        foreach (var anchored in _map.GetAnchoredEntities(grid, gridComp, tile))
        {
            // The generator doesnt count as blocking its own tile
            // Would be pretty awkward if the generator blocked itself
            // help
            if (anchored != uid
                && _physicsQuery.TryComp(anchored, out var body)
                && body is { BodyType: BodyType.Static, Hard: true }
                && (body.CollisionLayer & (int) CollisionGroup.Impassable) != 0)
                return Loc.GetString("advanced-anomaly-generator-error-blocked-location");
        }

        coords = _map.GridTileToLocal(grid, gridComp, tile);
        return null;
    }

    private void Announce(Entity<AdvancedAnomalyGeneratorComponent> ent, EntityUid anomaly, string anomalyName, EntityUid? user)
    {
        var operatorName = Loc.GetString("advanced-anomaly-generator-announce-unknown-user");
        if (user is { } actor)
        {
            operatorName = _idCard.TryFindIdCard(actor, out var idCard) && idCard.Comp.FullName is { Length: > 0 } fullName
                ? fullName
                : Name(actor);
        }

        var location = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(anomaly));
        var message = Loc.GetString("advanced-anomaly-generator-announce",
            ("anomaly", anomalyName), ("user", operatorName), ("location", location));

        _radio.SendRadioMessage(ent, message, ent.Comp.AnnouncementChannel, ent);
    }

    private void Report(Entity<AdvancedAnomalyGeneratorComponent> ent, EntityUid? user, string message)
    {
        if (user != null)
            _popup.PopupEntity(message, ent, user.Value);

        UpdateUi(ent);
    }

    private void UpdateUi(Entity<AdvancedAnomalyGeneratorComponent> ent)
    {
        var xform = Transform(ent);
        var machineTile = xform.GridUid is { } grid && TryComp<MapGridComponent>(grid, out var gridComp)
            ? _map.TileIndicesFor(grid, gridComp, xform.Coordinates)
            : Vector2i.Zero;

        var points = _research.TryGetClientServer(ent, out _, out var server) ? server.Points : 0;

        _ui.SetUiState(ent.Owner, AdvancedAnomalyGeneratorUiKey.Key, new AdvancedAnomalyGeneratorUserInterfaceState(
            new(ent.Comp.AllowedAnomalies),
            ent.Comp.RequiredMaterial,
            _material.GetMaterialAmount(ent, ent.Comp.RequiredMaterial),
            points,
            this.IsPowered(ent, EntityManager) && !HasComp<GeneratingAdvancedAnomalyGeneratorComponent>(ent),
            ent.Comp.CooldownEnd,
            ent.Comp.Range,
            GetNetEntity(xform.GridUid),
            machineTile));
    }
}
