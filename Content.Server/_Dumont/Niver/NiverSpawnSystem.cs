// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Decals;
using Content.Server.Station.Events;
using Content.Shared._Dumont.CCVar;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Placeable;
using Content.Trauma.Shared.Areas;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Dumont.Niver;

public sealed partial class NiverSpawnSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private DecalSystem _decals = default!;
    [Dependency] private AreaSystem _area = default!;

    private bool _enabled;
    private float _chance;

    private readonly List<EntityCoordinates> _barTables = new();
    private readonly List<EntityCoordinates> _barFloor = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<StationPostInitEvent>(OnStationPostInit);
        Subs.CVar(_cfg, DumontCVars.NiverEnabled, value => _enabled = value, true);
        Subs.CVar(_cfg, DumontCVars.NiverChancePerTile, value => _chance = value, true);
    }

    // Only fires for real stations in a round, so mapping mode never gets the spawns saved into the map.
    private void OnStationPostInit(ref StationPostInitEvent ev)
    {
        if (!_enabled || HasComp<NiverSpawnBlacklistComponent>(ev.Station))
            return;

        foreach (var grid in ev.Station.Comp.Grids)
        {
            if (IsBlacklisted(grid))
                continue;

            if (!TryComp<MapGridComponent>(grid, out var gridComp))
                continue;

            var spawner = EnsureComp<NiverSpawnComponent>(grid);
            SpawnOnGrid((grid, spawner, gridComp));
        }
    }

    private bool IsBlacklisted(EntityUid grid)
    {
        if (HasComp<NiverSpawnBlacklistComponent>(grid))
            return true;

        return Transform(grid).MapUid is { } map && HasComp<NiverSpawnBlacklistComponent>(map);
    }

    private void SpawnOnGrid(Entity<NiverSpawnComponent, MapGridComponent> ent)
    {
        var (uid, comp, grid) = ent;
        _barTables.Clear();
        _barFloor.Clear();

        foreach (var tileRef in _map.GetAllTiles(uid, grid))
        {
            if (_turf.IsSpace(tileRef))
                continue;

            // Impassable instead of Opaque so windows count too.
            if (_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable))
                continue;

            var center = _turf.GetTileCenter(tileRef);
            if (IsInArea(uid, center, comp.BarArea))
            {
                if (HasTable((uid, grid), tileRef.GridIndices))
                    _barTables.Add(center);
                else
                    _barFloor.Add(center);
            }

            if (_random.Prob(_chance))
            {
                var list = _random.Prob(comp.HornChance)
                    ? comp.HornPrototypes
                    : _random.Prob(0.5f) ? comp.PresentPrototypes : comp.BalloonPrototypes;

                if (list.Count > 0)
                    SpawnAtPosition(_random.Pick(list), center);
            }

            if (_random.Prob(comp.ConfettiChance))
            {
                var decalCoords = new EntityCoordinates(uid, tileRef.GridIndices);
                _decals.TryAddDecal(comp.ConfettiDecal, decalCoords, out _, rotation: _random.NextAngle(), cleanable: true);
            }
        }

        SpawnBarItems(comp);
    }

    private void SpawnBarItems(NiverSpawnComponent comp)
    {
        var spots = _barTables.Count > 0 ? _barTables : _barFloor;
        if (spots.Count == 0)
            return;

        foreach (var proto in comp.BarPrototypes)
        {
            SpawnAtPosition(proto, _random.Pick(spots));
        }
    }

    private bool IsInArea(EntityUid grid, EntityCoordinates coords, EntProtoId areaProto)
    {
        return _area.GetArea(grid, coords) is { } area && _area.GetAreaPrototype(area) == areaProto;
    }

    private bool HasTable(Entity<MapGridComponent> grid, Vector2i indices)
    {
        var anchored = _map.GetAnchoredEntities(grid, indices);
        while (anchored.MoveNext(out var anchoredUid))
        {
            if (HasComp<PlaceableSurfaceComponent>(anchoredUid))
                return true;
        }

        return false;
    }
}
