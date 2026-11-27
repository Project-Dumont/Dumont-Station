using Content.Server.Atmos.EntitySystems;
using Content.Shared._Dumont.Curse.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Curse;

public sealed class CurseRandomIgniteSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseRandomIgniteComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CurseRandomIgniteComponent> ent, ref MapInitEvent args)
    {
        Schedule(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CurseRandomIgniteComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var comp, out var damageable))
        {
            if (now < comp.NextIgnite)
                continue;

            Schedule(comp);

            if (!_mobState.IsAlive(uid) || damageable.TotalDamage > comp.MaxDamage)
                continue;

            _flammable.AdjustFireStacks(uid, comp.FireStacks, ignite: true);
            _popup.PopupEntity(Loc.GetString("curse-ignite-popup"), uid, uid, PopupType.MediumCaution);
        }
    }

    private void Schedule(CurseRandomIgniteComponent comp)
    {
        comp.NextIgnite = _timing.CurTime + _random.Next(comp.MinInterval, comp.MaxInterval);
    }
}
