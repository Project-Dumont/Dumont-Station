using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Genetics;
using Robust.Shared.Random;

namespace Content.Server.Genetics.System;

public sealed class IncendiaryMitochondriaSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IncendiaryMitochondriaGenComponent>();
        while (query.MoveNext(out var uid, out var incendiaryMitochondria))
        {
            if (incendiaryMitochondria.NextTimeTick <= 0)
            {
                incendiaryMitochondria.NextTimeTick = 60;
                if (_random.Next(0, 100) < 50)
                {
                    if (TryComp(uid, out FlammableComponent? flammable))
                    {
                        flammable.FireStacks = 1f;
                        _flammable.Ignite(uid, uid);
                    }
                }
            }
            incendiaryMitochondria.NextTimeTick -= frameTime;
        }
    }
}

