using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Trauma.Server.EntityEffects;
using Content.Trauma.Server.Spawners;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class LavalandSpawnRuleSystem : GameRuleSystem<LavalandSpawnRuleComponent>
{
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private LavalandSystem _lavaland = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private StationSpawningSystem _spawning = default!;
    [Dependency] private TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning, before: [ typeof(SpawnPointSystem), typeof(DropPodSpawnPointSystem) ]);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null || GameTicker.RunLevel == GameRunLevel.InRound)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            comp.Tiles ??= GetOutpostTiles();
            if (comp.Tiles.Count == 0)
                return;

            args.SpawnResult = _spawning.SpawnPlayerMob(RobustRandom.Pick(comp.Tiles),
                args.Job,
                args.HumanoidCharacterProfile,
                args.Station);
            return;
        }
    }

    private List<EntityCoordinates> GetOutpostTiles()
    {
        var tiles = new List<EntityCoordinates>();
        foreach (var lavaland in _lavaland.GetLavalands())
        {
            foreach (var grid in lavaland.Comp.SpawnedGrids)
            {
                if (!TryComp<MapGridComponent>(grid, out var gridComp))
                    continue;

                foreach (var tile in _map.GetAllTiles(grid, gridComp))
                {
                    if (_turf.IsTileBlocked(tile, CollisionGroup.MobMask) ||
                        TeleportRandomAreaSystem.IsMixtureUnsafe(_atmos.GetTileMixture(grid, lavaland.Owner, tile.GridIndices)))
                        continue;

                    tiles.Add(_map.GridTileToLocal(grid, gridComp, tile.GridIndices));
                }
            }
        }

        return tiles;
    }
}
