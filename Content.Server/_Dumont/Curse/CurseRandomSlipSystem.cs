using Content.Shared._Dumont.Curse.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Slippery;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Curse;

public sealed class CurseRandomSlipSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SlipperySystem _slippery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseRandomSlipComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CurseRandomSlipComponent> ent, ref MapInitEvent args)
    {
        Schedule(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CurseRandomSlipComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextSlip)
                continue;

            Schedule(comp);

            if (!_mobState.IsAlive(uid))
                continue;

            // same trick as the super slip smite, slip on ourselves
            var hadSlippery = EnsureComp<SlipperyComponent>(uid, out var slippery);
            _slippery.TrySlip(uid, slippery, uid, requiresContact: false);
            if (!hadSlippery)
                RemComp(uid, slippery);
        }
    }

    private void Schedule(CurseRandomSlipComponent comp)
    {
        comp.NextSlip = _timing.CurTime + _random.Next(comp.MinInterval, comp.MaxInterval);
    }
}
