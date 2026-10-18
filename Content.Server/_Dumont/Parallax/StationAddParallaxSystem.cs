using Content.Server.Station.Systems;
using Content.Shared._Dumont.EntityEffects;
using Content.Shared._Dumont.Parallax;
using Content.Shared.EntityEffects;

namespace Content.Server._Dumont.Parallax;

public sealed partial class StationAddParallaxSystem : EntitySystem
{
    [Dependency] private StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationAddParallax>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationAddParallax> args)
    {
        if (_station.GetLargestGrid(args.Args.TargetEntity) is not { } grid || Transform(grid).MapUid is not { } map)
            return;

        var comp = EnsureComp<ExtraParallaxComponent>(map);
        if (comp.Parallaxes.Contains(args.Effect.Parallax))
            return;

        comp.Parallaxes.Add(args.Effect.Parallax);
        Dirty(map, comp);
    }
}
