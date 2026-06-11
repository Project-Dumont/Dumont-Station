// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Anomaly.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Pinpointer;
using Content.Server.Power.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.Research.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Prototypes;
using Content.Shared.Materials;
using Content.Shared.Physics;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Research.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Anomaly;

public sealed class AdvancedAnomalyGeneratorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _material = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, MaterialAmountChangedEvent>(OnMaterialChanged);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, AdvancedAnomalyGeneratorGenerateMessage>(OnGenerate);
        SubscribeLocalEvent<AdvancedAnomalyGeneratorComponent, ResearchServerPointsChangedEvent>(OnResearchPointsChanged);
        SubscribeLocalEvent<GeneratingAdvancedAnomalyGeneratorComponent, ComponentStartup>(OnGeneratingStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneratingAdvancedAnomalyGeneratorComponent, AdvancedAnomalyGeneratorComponent>();
        while (query.MoveNext(out var uid, out var generating, out var gen))
        {
            if (_timing.CurTime < generating.EndTime)
                continue;

            generating.AudioStream = _audio.Stop(generating.AudioStream);
            OnGeneratingFinished(uid, generating, gen);
        }
    }

    private void OnGeneratingStartup(EntityUid uid, GeneratingAdvancedAnomalyGeneratorComponent component, ComponentStartup args)
    {
        _appearance.SetData(uid, AdvancedAnomalyGeneratorVisualLayers.Base, true);
    }

    private void OnGeneratingFinished(EntityUid uid, GeneratingAdvancedAnomalyGeneratorComponent generating, AdvancedAnomalyGeneratorComponent component)
    {
        _appearance.SetData(uid, AdvancedAnomalyGeneratorVisualLayers.Base, false);
        RemComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid);

        string message;
        if (!_prototype.TryIndex(generating.EntryId, out var entry))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-invalid-anomaly");
            RefundPlasmaAndFail(uid, component, generating, message);
            return;
        }

        if (!TryGetValidatedCoordinates(uid, generating.Tile, out var coords, out message))
        {
            RefundPlasmaAndFail(uid, component, generating, message);
            return;
        }

        if (!TryGetResearchServer(uid, out var serverUid, out var server) || serverUid is not { } researchServer)
        {
            message = Loc.GetString("advanced-anomaly-generator-error-no-server");
            RefundPlasmaAndFail(uid, component, generating, message);
            return;
        }

        _research.ModifyServerPoints(researchServer, -entry.ResearchCost, server);
        var anomaly = Spawn(entry.AnomalyPrototype, coords);
        component.NextSpawnTime = _timing.CurTime + component.CooldownLength;
        _audio.PlayPvs(component.GeneratingFinishedSound, uid);

        AnnounceGeneration(uid, component, anomaly, Loc.GetString(entry.Name), generating.User);

        message = Loc.GetString("advanced-anomaly-generator-success", ("anomaly", Loc.GetString(entry.Name)), ("x", generating.Tile.X), ("y", generating.Tile.Y));
        component.LastMessage = message;
        if (generating.User != null)
            _popup.PopupEntity(message, uid, generating.User.Value);

        UpdateUi(uid, component);
    }

    private void RefundPlasmaAndFail(EntityUid uid, AdvancedAnomalyGeneratorComponent component, GeneratingAdvancedAnomalyGeneratorComponent generating, string message)
    {
        _material.TryChangeMaterialAmount(uid, component.RequiredMaterial, generating.PlasmaConsumed);
        Fail(uid, component, generating.User, message);
    }

    private void OnUiOpened(EntityUid uid, AdvancedAnomalyGeneratorComponent component, BoundUIOpenedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnMaterialChanged(EntityUid uid, AdvancedAnomalyGeneratorComponent component, ref MaterialAmountChangedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnResearchPointsChanged(EntityUid uid, AdvancedAnomalyGeneratorComponent component, ref ResearchServerPointsChangedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnGenerate(EntityUid uid, AdvancedAnomalyGeneratorComponent component, AdvancedAnomalyGeneratorGenerateMessage args)
    {
        if (HasComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid))
            return;

        TryBeginGeneration(uid, args.Actor, args.EntryId, new Vector2i(args.TileX, args.TileY), component);
    }

    private void TryBeginGeneration(EntityUid uid, EntityUid? user, string entryId, Vector2i tile, AdvancedAnomalyGeneratorComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        string message;
        if (!this.IsPowered(uid, EntityManager))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-unpowered");
            Fail(uid, component, user, message);
            return;
        }

        if (_timing.CurTime < component.NextSpawnTime)
        {
            var remaining = component.NextSpawnTime - _timing.CurTime;
            message = Loc.GetString("advanced-anomaly-generator-error-cooldown",
                ("time", $"{(int) remaining.TotalMinutes:D2}:{remaining.Seconds:D2}"));
            Fail(uid, component, user, message);
            return;
        }

        if (!_prototype.TryIndex<AdvancedAnomalyGenerationPrototype>(entryId, out var entry) || !component.AllowedAnomalies.Contains(entry.ID))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-invalid-anomaly");
            Fail(uid, component, user, message);
            return;
        }

        if (!_prototype.HasIndex<EntityPrototype>(entry.AnomalyPrototype))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-invalid-anomaly");
            Fail(uid, component, user, message);
            return;
        }

        var plasmaCost = entry.PlasmaCost ?? component.MaterialCost;
        var plasma = _material.GetMaterialAmount(uid, component.RequiredMaterial);
        if (plasma < plasmaCost)
        {
            message = Loc.GetString("advanced-anomaly-generator-error-plasma", ("needed", plasmaCost), ("available", plasma));
            Fail(uid, component, user, message);
            return;
        }

        if (!TryGetResearchServer(uid, out _, out var server))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-no-server");
            Fail(uid, component, user, message);
            return;
        }

        if (server.Points < entry.ResearchCost)
        {
            message = Loc.GetString("advanced-anomaly-generator-error-research", ("needed", entry.ResearchCost), ("available", server.Points));
            Fail(uid, component, user, message);
            return;
        }

        if (!TryGetValidatedCoordinates(uid, tile, out _, out message))
        {
            Fail(uid, component, user, message);
            return;
        }

        if (!_material.TryChangeMaterialAmount(uid, component.RequiredMaterial, -plasmaCost))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-plasma", ("needed", plasmaCost), ("available", plasma));
            Fail(uid, component, user, message);
            return;
        }

        var generating = EnsureComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid);
        generating.EndTime = _timing.CurTime + component.GenerationLength;
        generating.EntryId = entryId;
        generating.Tile = tile;
        generating.User = user;
        generating.PlasmaConsumed = plasmaCost;
        generating.AudioStream = _audio.PlayPvs(component.GeneratingSound, uid, AudioParams.Default.WithLoop(true))?.Entity;

        UpdateUi(uid, component);
    }

    private void UpdateUi(EntityUid uid, AdvancedAnomalyGeneratorComponent component)
    {
        var entries = GetEntries(component)
            .Select(p => new AdvancedAnomalyGeneratorEntryState(
                p.ID,
                Loc.GetString(p.Name),
                p.ResearchCost,
                p.PlasmaCost ?? component.MaterialCost,
                p.AnomalyPrototype))
            .ToList();

        var plasma = _material.GetMaterialAmount(uid, component.RequiredMaterial);
        var points = TryGetResearchServer(uid, out _, out var server) ? server.Points : 0;
        var stationGrid = TryGetStationGrid(uid, out var grid) ? grid : null;
        if (stationGrid is { } gridUid)
            EnsureComp<NavMapComponent>(gridUid);

        var defaultTile = GetDefaultTile(uid);
        var netGrid = stationGrid is { } validGrid ? GetNetEntity(validGrid) : NetEntity.Invalid;
        var canUse = this.IsPowered(uid, EntityManager) && !HasComp<GeneratingAdvancedAnomalyGeneratorComponent>(uid);

        _ui.SetUiState(uid, AdvancedAnomalyGeneratorUiKey.Key,
            new AdvancedAnomalyGeneratorUserInterfaceState(entries, plasma, component.MaterialCost, points,
                component.LastMessage, canUse, defaultTile.X, defaultTile.Y, netGrid));
    }

    private IEnumerable<AdvancedAnomalyGenerationPrototype> GetEntries(AdvancedAnomalyGeneratorComponent component)
    {
        foreach (var id in component.AllowedAnomalies)
        {
            if (_prototype.TryIndex(id, out AdvancedAnomalyGenerationPrototype? proto))
                yield return proto;
        }
    }

    private Vector2i GetDefaultTile(EntityUid uid)
    {
        var xform = Transform(uid);
        if (!TryGetStationGrid(uid, out var grid) || !TryComp<MapGridComponent>(grid.Value, out var gridComp))
            return Vector2i.Zero;

        return _map.LocalToTile(grid.Value, gridComp, xform.Coordinates);
    }

    private bool TryGetValidatedCoordinates(EntityUid uid, Vector2i tile, out EntityCoordinates coords, out string message)
    {
        coords = default;
        var xform = Transform(uid);
        if (!TryGetStationGrid(uid, out var grid))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-no-station");
            return false;
        }

        if (xform.GridUid != grid)
        {
            message = Loc.GetString("advanced-anomaly-generator-error-wrong-grid");
            return false;
        }

        var gridUid = grid.Value;
        if (!TryComp<MapGridComponent>(gridUid, out var gridComp) || !_map.TryGetTileRef(gridUid, gridComp, tile, out var tileRef) || tileRef.Tile.IsEmpty)
        {
            message = Loc.GetString("advanced-anomaly-generator-error-invalid-location");
            return false;
        }

        if (_atmosphere.IsTileSpace(gridUid, xform.MapUid, tile) || _atmosphere.IsTileAirBlocked(gridUid, tile, mapGridComp: gridComp))
        {
            message = Loc.GetString("advanced-anomaly-generator-error-invalid-location");
            return false;
        }

        var physQuery = GetEntityQuery<PhysicsComponent>();
        foreach (var ent in _map.GetAnchoredEntities(gridUid, gridComp, tile))
        {
            if (!physQuery.TryGetComponent(ent, out var body))
                continue;
            if (body.BodyType == BodyType.Static && body.Hard && (body.CollisionLayer & (int) CollisionGroup.Impassable) != 0)
            {
                message = Loc.GetString("advanced-anomaly-generator-error-blocked-location");
                return false;
            }
        }

        coords = _map.GridTileToLocal(gridUid, gridComp, tile);
        message = string.Empty;
        return true;
    }

    private bool TryGetStationGrid(EntityUid uid, [NotNullWhen(true)] out EntityUid? grid)
    {
        var xform = Transform(uid);
        if (_station.GetStationInMap(xform.MapID) is { } station &&
            _station.GetLargestGrid(station) is { } stationGrid)
        {
            grid = stationGrid;
            return true;
        }

        grid = xform.GridUid;
        return grid != null;
    }

    private bool TryGetResearchServer(EntityUid uid, out EntityUid? serverUid, [NotNullWhen(true)] out ResearchServerComponent? server)
    {
        return _research.TryGetClientServer(uid, out serverUid, out server);
    }

    private void AnnounceGeneration(EntityUid uid, AdvancedAnomalyGeneratorComponent component, EntityUid anomaly, string anomalyName, EntityUid? user)
    {
        var who = Loc.GetString("advanced-anomaly-generator-announce-unknown-user");
        if (user is { } actor)
        {
            if (_idCard.TryFindIdCard(actor, out var idCard) && !string.IsNullOrWhiteSpace(idCard.Comp.FullName))
                who = idCard.Comp.FullName;
            else
                who = Name(actor);
        }

        var location = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((anomaly, Transform(anomaly))));

        var announcement = Loc.GetString("advanced-anomaly-generator-announce",
            ("anomaly", anomalyName),
            ("user", who),
            ("location", location));

        _radio.SendRadioMessage(uid, announcement, _prototype.Index<RadioChannelPrototype>(component.AnnouncementChannel), uid);
    }

    private void Fail(EntityUid uid, AdvancedAnomalyGeneratorComponent component, EntityUid? user, string message)
    {
        component.LastMessage = message;
        if (user != null)
            _popup.PopupEntity(message, uid, user.Value);
        UpdateUi(uid, component);
    }
}
