using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed class SharedXenomorphVenatorSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVenatorComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem) });
    }
    private void OnDamage(Entity<XenomorphVenatorComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.AcidCharges > 0) args.Damage *= 1 + ent.Comp.AcidCharges * .05f;
    }
}
