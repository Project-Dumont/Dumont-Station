// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Dumont.EntityEffects;
using Content.Server.Station.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Station.Components;
using Content.Trauma.Shared.EntityEffects.Station;

namespace Content.Trauma.Server.EntityEffects.Station;

public sealed partial class StationQueryEffectsSystem : EntitySystem
{
    [Dependency] private EntityEffectListSystem _effects = default!;
    [Dependency] private StationSystem _station = default!;

    private List<EntityUid> _targets = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationQueryEffects>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationQueryEffects> args)
    {
        var station = args.Args.TargetEntity;
        if (!HasComp<StationDataComponent>(station))
            return;

        var e = args.Effect;
        var type = Factory.GetRegistration(e.CompName).Type;
        var effects = e.Effects;

        // Dumont changes start
        _targets.Clear();
        foreach (var (uid, _) in EntityManager.GetAllComponents(type, e.IncludePaused))
        {
            if (_station.GetOwningStation(uid) != station)
                continue;

            _targets.Add(uid);
        }

        foreach (var uid in _targets)
        {
            _effects.ApplyEffects(uid, effects);
        }
        // Dumont end
    }
}
