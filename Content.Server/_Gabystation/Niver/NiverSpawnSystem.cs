// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;
using Content.Shared._Gabystation.CCVar;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Server.Station.Components;

namespace Content.Server._Gabystation.Niver;

public sealed class NiverSpawnSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private float _chance = 1f;

    public override void Initialize()
    {
        SubscribeLocalEvent<NiverSpawnComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BecomesStationComponent, ComponentStartup>(OnStationInit);
        Subs.CVar(_cfg, GabyCVars.NiverChancePerTile, value => _chance = value, true);
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
        }
    }
}
