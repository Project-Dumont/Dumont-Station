using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Body.Events;
using Content.Shared.Damage;
using Content.Shared.Spawners.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared._Trauma.BloodSplatter;

public sealed class SharedBloodSplatterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    private static readonly EntProtoId SlashProto = "Slash";
    private static readonly EntProtoId PierceProto = "Piercing";

    public override void Initialize()
    {
        base.Initialize();

        if (_net.IsClient)
            return;

        SubscribeLocalEvent<BloodSplattererComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<BloodSplattererComponent, BeingGibbedEvent>(OnGib);
    }

    private void OnGib(Entity<BloodSplattererComponent> ent, ref BeingGibbedEvent args)
    {
        if (!TryComp<BloodstreamComponent>(ent.Owner, out var bloodstream))
            return;

        SpawnDecal(ent, bloodstream, ent.Comp.GibbedDecal);
    }

    private void OnDamage(Entity<BloodSplattererComponent> ent, ref DamageChangedEvent args)
    {
        var time = _timing.CurTime;

        if (ent.Comp.NextSplashAvailable > time)
            return;

        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        args.DamageDelta.DamageDict.TryGetValue(PierceProto, out var piercing);
        args.DamageDelta.DamageDict.TryGetValue(SlashProto, out var slash);

        var sharpDamage = piercing + slash;
        if (sharpDamage < ent.Comp.MinimalTriggerDamage)
            return;

        if (!TryComp<BloodstreamComponent>(ent.Owner, out var bloodstream)
            || _bloodstream.GetBloodLevelPercentage((ent.Owner, bloodstream)) <= 0.5f)
            return;

        var splatterChance = Math.Min(1f, ent.Comp.Chance + (float) sharpDamage / 50); // Higher damage has higher change to splatter

        if (!_random.Prob(splatterChance))
            return;

        SpawnDecal(ent, bloodstream, ent.Comp.Decal);

        ent.Comp.NextSplashAvailable = _timing.CurTime + ent.Comp.SplashCooldown;
    }

    private void SpawnDecal(Entity<BloodSplattererComponent> ent, BloodstreamComponent bloodstream, string decal)
    {
        var spawnedDecal = EntityManager.CreateEntityUninitialized(decal, ent.Owner.ToCoordinates());

        if (TryComp<RandomDecalSpawnerComponent>(spawnedDecal, out var randomDecal))
        {
            randomDecal.Color = _prototypes.Index(bloodstream.BloodReagent).SubstanceColor;
        }

        EntityManager.InitializeAndStartEntity(spawnedDecal);

        _audio.PlayPvs(ent.Comp.SplatSound, ent.Owner);
    }
}
