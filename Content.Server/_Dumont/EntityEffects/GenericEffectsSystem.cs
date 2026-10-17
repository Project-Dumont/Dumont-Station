using Content.Shared._Dumont.EntityEffects;
using Content.Shared.EntityEffects;

namespace Content.Server._Dumont.EntityEffects;

public sealed partial class GenericEffectsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<AddComponents>>(OnAddComponents);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<RemoveComponents>>(OnRemoveComponents);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<DeleteTarget>>(OnDeleteTarget);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<SpawnAtTarget>>(OnSpawnAtTarget);
    }

    private void OnAddComponents(ref ExecuteEntityEffectEvent<AddComponents> args)
    {
        EntityManager.AddComponents(args.Args.TargetEntity, args.Effect.Components);
    }

    private void OnRemoveComponents(ref ExecuteEntityEffectEvent<RemoveComponents> args)
    {
        EntityManager.RemoveComponents(args.Args.TargetEntity, args.Effect.Components);
    }

    private void OnDeleteTarget(ref ExecuteEntityEffectEvent<DeleteTarget> args)
    {
        QueueDel(args.Args.TargetEntity);
    }

    private void OnSpawnAtTarget(ref ExecuteEntityEffectEvent<SpawnAtTarget> args)
    {
        var coords = Transform(args.Args.TargetEntity).Coordinates;
        for (var i = 0; i < args.Effect.Number; i++)
        {
            SpawnAttachedTo(args.Effect.Entity, coords);
        }
    }
}
