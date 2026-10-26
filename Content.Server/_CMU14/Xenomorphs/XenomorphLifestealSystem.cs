using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Body.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphLifestealSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    private readonly HashSet<Entity<MobStateComponent>> _targets = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphLifestealComponent, MeleeHitEvent>(OnHit);
    }

    private bool IsPrey(EntityUid target)
    {
        return HasComp<BodyComponent>(target) && !HasComp<XenomorphComponent>(target)
            && _mobState.IsAlive(target)
            && (!TryComp<BuckleComponent>(target, out var buckle) || !buckle.Buckled);
    }

    private void OnHit(Entity<XenomorphLifestealComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner || !_mobState.IsAlive(ent)
            || !args.HitEntities.Any(IsPrey)
            || !TryComp<DamageableComponent>(ent, out var damage) || damage.TotalDamage <= 0)
            return;

        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.TargetRange, _targets);
        var percentage = ent.Comp.BasePercentage;
        foreach (var target in _targets)
        {
            if (IsPrey(target))
                percentage = FixedPoint2.Min(ent.Comp.MaxPercentage, percentage + ent.Comp.TargetIncreasePercentage);
        }

        var amount = FixedPoint2.Min(damage.TotalDamage,
            FixedPoint2.Clamp(damage.TotalDamage * percentage, ent.Comp.MinHeal, ent.Comp.MaxHeal));
        var healing = new DamageSpecifier();
        foreach (var (type, value) in damage.Damage.DamageDict)
        {
            if (value > 0)
                healing.DamageDict[type] = -(amount * value / damage.TotalDamage);
        }
        _damage.TryChangeDamage(ent, healing, true, origin: ent);
    }
}
