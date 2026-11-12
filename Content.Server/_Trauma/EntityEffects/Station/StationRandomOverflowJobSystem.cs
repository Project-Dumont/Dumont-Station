// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects.Station;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects.Station;

public sealed partial class StationRandomOverflowJobSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StationJobsSystem _stationJobs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationRandomOverflowJob>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationRandomOverflowJob> args)
    {
        var station = args.Args.TargetEntity;
        if (!TryComp<StationJobsComponent>(station, out var stationJobs))
            return;

        var ignored = args.Effect.IgnoredJobs;
        var jobs = new List<string>();
        foreach (var (job, slots) in stationJobs.JobList)
        {
            if (ignored.Contains(job) ||
                // don't be boring and change nothing for jobs that are already unlimited
                slots == null)
                continue;

            jobs.Add(job);
        }

        if (jobs.Count == 0)
            return;

        var picked = _random.Pick(jobs);
        _stationJobs.MakeJobUnlimited(station, picked, stationJobs);
    }
}
