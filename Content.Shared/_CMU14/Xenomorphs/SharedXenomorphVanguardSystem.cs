using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.Explosion;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphVanguardSystem : EntitySystem
{
    [Dependency] private SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVanguardComponent, GetExplosionResistanceEvent>(OnExplosion);
        SubscribeLocalEvent<XenomorphVanguardComponent, StopThrowEvent>(OnStop, before: new[] { typeof(XenomorphLeapSystem) });
    }

    private void OnExplosion(Entity<XenomorphVanguardComponent> ent, ref GetExplosionResistanceEvent args)
    {
        if (ent.Comp.Shield > 0) args.DamageCoefficient /= (float) Math.Pow(1.1, 75 / 5.0);
    }

    private void OnStop(Entity<XenomorphVanguardComponent> ent, ref StopThrowEvent args)
    {
        if (TryComp<XenomorphLeapComponent>(ent, out var leap) && leap.Leaping) _physics.SetLinearVelocity(ent, Vector2.Zero);
    }
}
