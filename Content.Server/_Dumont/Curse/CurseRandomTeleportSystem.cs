using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared._Dumont.Curse.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Curse;

public sealed class CurseRandomTeleportSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRandomTeleportSystem _teleport = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseRandomTeleportComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CurseRandomTeleportComponent> ent, ref MapInitEvent args)
    {
        Schedule(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CurseRandomTeleportComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextTeleport)
                continue;

            Schedule(comp);

            if (_mobState.IsAlive(uid))
                _teleport.RandomTeleport(uid, comp);
        }
    }

    private void Schedule(CurseRandomTeleportComponent comp)
    {
        comp.NextTeleport = _timing.CurTime + _random.Next(comp.MinInterval, comp.MaxInterval);
    }
}
