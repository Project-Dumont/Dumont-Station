// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects.Station;

namespace Content.Trauma.Server.EntityEffects.Station;

public sealed partial class StationModifyJobsSystem : EntitySystem
{
    [Dependency] private StationJobsSystem _stationJobs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationModifyJobs>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationModifyJobs> args)
    {
        var station = args.Args.TargetEntity;
        if (!TryComp<StationJobsComponent>(station, out var jobs))
            return;

        foreach (var (job, add) in args.Effect.Add)
        {
            _stationJobs.TryAdjustJobSlot(station, job, add, stationJobs: jobs);
        }

        foreach (var (job, value) in args.Effect.Set)
        {
            _stationJobs.TrySetJobSlot(station, job, value, stationJobs: jobs);
        }
    }
}
