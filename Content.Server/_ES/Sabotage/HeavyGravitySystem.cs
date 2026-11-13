// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.Gravity;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Shared._ES.Sabotage;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Robust.Shared.Timing;

namespace Content.Server._ES.Sabotage;

public sealed partial class HeavyGravitySystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedHeavyGravitySystem _effect = default!;

    private TimeSpan _nextUpdate;

    public bool TryStart(EntityUid generator)
    {
        if (HasComp<HeavyGravitySabotageComponent>(generator)
            || _station.GetOwningStation(generator) is not {} station)
            return false;

        var comp = AddComp<HeavyGravitySabotageComponent>(generator);
        comp.Station = station;
        comp.ActivatesAt = _timing.CurTime + TimeSpan.FromMinutes(5);
        return true;
    }

    public void Stop(EntityUid generator)
    {
        RemComp<HeavyGravitySabotageComponent>(generator);
        RefreshEffects();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timing.CurTime < _nextUpdate)
            return;
        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(0.25);
        RefreshEffects();
    }

    private void RefreshEffects()
    {
        var grids = new HashSet<EntityUid>();
        var generators = EntityQueryEnumerator<HeavyGravitySabotageComponent, GravityGeneratorComponent, PowerChargeComponent>();
        while (generators.MoveNext(out var uid, out var sabotage, out var gravity, out var charge))
        {
            var xform = Transform(uid);
            if (_timing.CurTime < sabotage.ActivatesAt || !xform.Anchored
                || !gravity.GravityActive || !charge.Active
                || _station.GetOwningStation(uid) != sabotage.Station || xform.GridUid is not {} grid)
                continue;

            if (!sabotage.Announced)
            {
                _chat.DispatchStationAnnouncement(uid, Loc.GetString("es-sabotage-gravity-announcement"));
                sabotage.Announced = true;
            }
            grids.Add(grid);
        }

        var mobs = EntityQueryEnumerator<MobStateComponent, StandingStateComponent, TransformComponent>();
        while (mobs.MoveNext(out var uid, out _, out _, out var xform))
        {
            var affected = xform.GridUid is {} grid && grids.Contains(grid);
            if (affected || HasComp<HeavyGravityComponent>(uid))
                _effect.SetHeavyGravity(uid, affected);
        }
    }
}
