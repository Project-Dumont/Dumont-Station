// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Numerics;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Server._White.Spawners.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._White.Spawners.Systems;

public sealed partial class AreaSpawnerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MapSystem _map = default!;
    [Dependency] private TransformSystem _transform = default!;

    private readonly List<Vector2> _offsets = new List<Vector2>
    {
                           new Vector2(0, 1),
        new Vector2(-1, 0),                  new Vector2(1, 0),
                           new Vector2(0, -1)
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AreaSpawnerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, AreaSpawnerComponent component, ComponentShutdown args)
    {
        foreach (var spawned in component.Spawneds)
        {
            // <Goobstation> rewrote to be non goida
            if (TerminatingOrDeleted(spawned))
                continue;

            var comp = EnsureComp<TimedDespawnComponent>(spawned);
            comp.Lifetime = _random.NextFloat(component.MinTime, component.MaxTime);
            // </Goobstation>
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<AreaSpawnerComponent>();
        while (query.MoveNext(out var uid, out var areaSpawner))
        {
            if (time < areaSpawner.SpawnAt)
                continue;

            areaSpawner.SpawnAt = time + areaSpawner.SpawnDelay;

            var validTiles = GetValidTilesInRadius(uid, areaSpawner);

            foreach (var tile in validTiles)
            {
                var coordinates = Transform(uid).Coordinates.Offset(tile);
                if (areaSpawner.SpawnPrototype == "DumontHiveWeeds" && Transform(uid).GridUid is { } grid && TryComp<MapGridComponent>(grid, out var mapGrid))
                    foreach (var existing in _map.GetAnchoredEntities((grid, mapGrid), coordinates))
                        if (Prototype(existing)?.ID == "ResinWeed") QueueDel(existing);
                var spawnedUid = Spawn(areaSpawner.SpawnPrototype, coordinates);
                areaSpawner.Spawneds.Add(spawnedUid);
            }
        }
    }

    public List<Vector2> GetValidTilesInRadius(EntityUid uid, AreaSpawnerComponent component)
    {
        var validTiles = new List<Vector2>();
        for (var y = -component.Radius; y <= component.Radius; y++)
        {
            for (var x = -component.Radius; x <= component.Radius; x++)
            {
                var tile = new Vector2(x, y);
                if (IsTileValidForSpawn(uid, component, tile))
                    validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    public bool IsTileValidForSpawn(EntityUid uid, AreaSpawnerComponent component, Vector2 offset)
    {
        var xform = Transform(uid);
        if (_transform.GetGrid((uid, xform)) is not { } gridUid
            || !TryComp<MapGridComponent>(gridUid, out var mapGridComponent))
            return false;

        var coords = xform.Coordinates.Offset(offset);
        var tile = _turf.GetTileRef(coords);

        if (!tile.HasValue || tile.Value.Tile.IsEmpty)
            return false;

        foreach (var entity in _map.GetAnchoredEntities((gridUid, mapGridComponent), coords))
        {
            if (component.SpawnPrototype == "ResinWeed" && (HasComp<XenomorphHardyWeedComponent>(entity) || HasComp<XenomorphHiveWeedComponent>(entity))
                || component.SpawnPrototype == "DumontXenomorphHardyWeed" && HasComp<AreaSpawnerComponent>(entity))
                return false;

            if (TryComp<AirtightComponent>(entity, out var airtight) && airtight.AirBlocked
                || Prototype(entity) != null && Prototype(entity)! == component.SpawnPrototype
                || Prototype(entity) == Prototype(uid))
                return false;
        }

        foreach (var checkOffset in _offsets)
        {
            var checkCoords = coords.Offset(checkOffset);
            foreach (var entity in _map.GetAnchoredEntities((gridUid, mapGridComponent), checkCoords))
            {
                if (component.Spawneds.Contains(entity) || entity == uid)
                    return true;
            }
        }

        return false;
    }
}
