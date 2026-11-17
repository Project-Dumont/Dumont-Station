using Robust.Shared.Configuration;
using Content.Shared._Dumont.CCVar;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Server.Station.Components;
using Content.Server.Decals;

namespace Content.Server._Dumont.Niver;

public sealed partial class NiverSpawnSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private DecalSystem _decals = default!;

    private float _chance = 1f;

    public override void Initialize()
    {
        SubscribeLocalEvent<NiverSpawnComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BecomesStationComponent, ComponentStartup>(OnStationInit);
        Subs.CVar(_cfg, DumontCVars.NiverChancePerTile, value => _chance = value, true);
    }

    private void OnStationInit(Entity<BecomesStationComponent> ent, ref ComponentStartup args)
    {
        if (HasComp<NiverSpawnBlacklistComponent>(ent.Owner))
            return;

        EnsureComp<NiverSpawnComponent>(ent.Owner);
    }

    private void OnComponentInit(Entity<NiverSpawnComponent> ent, ref ComponentInit args)
    {
        var uid = ent.Owner;
        if (!TryComp<MapGridComponent>(uid, out var grid))
            return;

        var mapId = Transform(uid).MapID;
        if (mapId == MapId.Nullspace)
            return;

        foreach (var tileRef in _map.GetAllTiles(uid, grid))
        {
            if (_turf.IsSpace(tileRef))
                continue;
            if (_turf.IsTileBlocked(tileRef, CollisionGroup.Opaque))
                continue;

            if (_random.Prob(_chance))
            {
                var wantPresent = _random.Prob(0.5f);

                string proto;
                if (wantPresent)
                    proto = _random.Pick(ent.Comp.PresentPrototypes);
                else
                    proto = _random.Pick(ent.Comp.BalloonPrototypes);

                var coords = _turf.GetTileCenter(tileRef);
                Spawn(proto, coords);
            }

            if (_random.Prob(ent.Comp.ConfettiChance))
            {
                var decalCoords = new EntityCoordinates(uid, tileRef.GridIndices);
                _decals.TryAddDecal(ent.Comp.ConfettiDecal, decalCoords, out _, rotation: _random.NextAngle(), cleanable: true);
            }
        }
    }
}