using Content.Server.Decals;
using Content.Server.Station.Events;
using Content.Shared._Dumont.CCVar;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._Dumont.Niver;

public sealed partial class NiverSpawnSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private DecalSystem _decals = default!;

    private float _chance;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationPostInitEvent>(OnStationPostInit);
        Subs.CVar(_cfg, DumontCVars.NiverChancePerTile, value => _chance = value, true);
    }

    // Only fires for real stations in a round, so mapping mode never gets the spawns saved into the map.
    private void OnStationPostInit(ref StationPostInitEvent ev)
    {
        if (HasComp<NiverSpawnBlacklistComponent>(ev.Station))
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

        foreach (var tileRef in _map.GetAllTiles(uid, grid))
        {
            if (_turf.IsSpace(tileRef))
                continue;

            // Impassable instead of Opaque so windows count too.
            if (_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable))
                continue;

            if (_random.Prob(_chance))
            {
                var list = _random.Prob(0.5f) ? comp.PresentPrototypes : comp.BalloonPrototypes;
                if (list.Count > 0)
                    SpawnAtPosition(_random.Pick(list), _turf.GetTileCenter(tileRef));
            }

            if (_random.Prob(comp.ConfettiChance))
            {
                var decalCoords = new EntityCoordinates(uid, tileRef.GridIndices);
                _decals.TryAddDecal(comp.ConfettiDecal, decalCoords, out _, rotation: _random.NextAngle(), cleanable: true);
            }
        }
    }
}
