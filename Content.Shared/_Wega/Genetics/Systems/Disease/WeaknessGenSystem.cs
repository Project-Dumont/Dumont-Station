using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Melee;

namespace Content.Shared.Genetics.Systems;

public sealed class WeaknessSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeaknessGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WeaknessGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<WeaknessGenComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<MeleeWeaponComponent>(ent, out var melee))
            return;

        string damageType = melee.Damage.DamageDict.ContainsKey("Slash") ? "Slash" : "Blunt";
        if (melee.Damage.DamageDict.TryGetValue(damageType, out var currentDamage))
        {
            ent.Comp.OldDamage = currentDamage;
        }
        else
        {
            ent.Comp.OldDamage = FixedPoint2.Zero;
        }

        melee.Damage.DamageDict[damageType] = currentDamage - ent.Comp.WeaknessModifier;
    }

    private void OnShutdown(Entity<WeaknessGenComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MeleeWeaponComponent>(ent, out var melee))
            return;

        string damageType = melee.Damage.DamageDict.ContainsKey("Slash") ? "Slash" : "Blunt";
        if (melee.Damage.DamageDict.TryGetValue(damageType, out var currentDamage))
        {
            melee.Damage.DamageDict[damageType] = currentDamage + ent.Comp.OldDamage;
        }
    }
}

