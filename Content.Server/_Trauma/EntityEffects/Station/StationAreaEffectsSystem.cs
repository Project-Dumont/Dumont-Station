// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Dumont.EntityEffects;
using Content.Server.Station.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Station.Components;
using Content.Trauma.Shared.EntityEffects.Station;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects.Station;

public sealed partial class StationAreaEffectsSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private EntityEffectListSystem _effects = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private TurfSystem _turf = default!;

    private List<EntityUid> _areas = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationAreaEffects>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationAreaEffects> args)
    {
        var station = args.Args.TargetEntity;
        if (!HasComp<StationDataComponent>(station))
            return;

        var e = args.Effect;
        var type = Factory.GetRegistration(e.AreaComp).Type;

        // TODO: make a open areas cache somewhere...
        var mask = CollisionGroup.MobMask;
        _areas.Clear();
        foreach (var (uid, _) in EntityManager.GetAllComponents(type))
        {
            if (_station.GetOwningStation(uid) != station)
                continue;

            var coords = Transform(uid).Coordinates;
            if (e.CheckBlocked && (_turf.GetTileRef(coords) is not {} tile || _turf.IsTileBlocked(tile, mask)))
                continue;

            _areas.Add(uid);
        }

        var count = _random.Next(e.Min, e.Max);
        for (int i = 0; i < count; i++)
        {
            if (_areas.Count == 0)
                return;

            var area = _random.PickAndTake(_areas);
            _effects.ApplyEffects(area, e.Effects);
        }
    }
}
