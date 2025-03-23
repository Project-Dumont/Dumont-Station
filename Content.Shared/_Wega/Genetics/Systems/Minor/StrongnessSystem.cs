using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Melee;

namespace Content.Shared.Genetics.Systems;

public sealed class StrongnessSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StrongnessGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<StrongnessGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<StrongnessGenComponent> ent, ref ComponentInit args)
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

        melee.Damage.DamageDict[damageType] = currentDamage + ent.Comp.StrongnessModifier;
    }

    private void OnShutdown(Entity<StrongnessGenComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MeleeWeaponComponent>(ent, out var melee))
            return;

        string damageType = melee.Damage.DamageDict.ContainsKey("Slash") ? "Slash" : "Blunt";
        if (melee.Damage.DamageDict.TryGetValue(damageType, out _))
        {
            melee.Damage.DamageDict[damageType] = ent.Comp.OldDamage;
        }
    }
}

