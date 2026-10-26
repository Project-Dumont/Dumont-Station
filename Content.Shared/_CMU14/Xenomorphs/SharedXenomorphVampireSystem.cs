using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphVampireSystem : EntitySystem
{
    [Dependency] private SharedPhysicsSystem _physics = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVampireComponent, StopThrowEvent>(OnStop, before: new[] { typeof(XenomorphLeapSystem) });
    }
    private void OnStop(Entity<XenomorphVampireComponent> ent, ref StopThrowEvent args)
    {
        if (TryComp<XenomorphLeapComponent>(ent, out var leap) && leap.Leaping) _physics.SetLinearVelocity(ent, Vector2.Zero);
    }
}
