using Content.Shared.Mobs;
using Content.Shared.Random.Helpers;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._FarHorizons.Mobs;

public sealed class SharedMobSplittingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobSplittingComponent, MobStateChangedEvent>(OnStateChanged);
    }

    private void OnStateChanged(Entity<MobSplittingComponent> ent, ref MobStateChangedEvent args)
    {
        if (!(args.NewMobState == MobState.Dead && args.OldMobState != MobState.Dead) ||
            TerminatingOrDeleted(ent))
            return;

        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, ent.Owner);

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        foreach (var (entProto, spawnCfg) in ent.Comp.SplitInto)
        {
            var spawnAmount = rand.Next(spawnCfg.Min, spawnCfg.Max + 1);

            for (var i = 0; i < spawnAmount; i++)
            {
                if (!PredictedTrySpawnNextTo(entProto, ent, out var spawned))
                    continue;
                
                var throwTarget = rand.NextAngle().ToWorldVec();

                _throw.TryThrow(spawned.Value, throwTarget);
            }
        }
    }
}