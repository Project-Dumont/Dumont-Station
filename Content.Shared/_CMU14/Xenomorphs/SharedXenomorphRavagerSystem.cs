using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphRavagerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRavagerComponent, GetMeleeDamageEvent>(OnMeleeDamage);
        SubscribeLocalEvent<XenomorphRavagerComponent, XenomorphTailDamageEvent>(OnTailDamage);
    }

    private void OnMeleeDamage(Entity<XenomorphRavagerComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (_timing.CurTime < ent.Comp.SuperUntil)
            args.Damage += new DamageSpecifier { DamageDict = { ["Blunt"] = 0.66, ["Slash"] = 0.67, ["Piercing"] = 0.67 } } * ent.Comp.EmpoweredTargets;
    }

    private void OnTailDamage(Entity<XenomorphRavagerComponent> ent, ref XenomorphTailDamageEvent args)
    {
        if (_timing.CurTime < ent.Comp.SuperUntil)
            args.Damage += new DamageSpecifier { DamageDict = { ["Blunt"] = 0.8, ["Slash"] = 0.8, ["Piercing"] = 0.8 } } * ent.Comp.EmpoweredTargets;
    }
}
