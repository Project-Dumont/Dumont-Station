using Content.Shared.Damage.Components;
using Content.Shared.Genetics;

namespace Content.Server.Genetics.System;

public sealed class RegenerationGenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RegenerationGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RegenerationGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<RegenerationGenComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<PassiveDamageComponent>(ent, out var damage))
            return;

        ent.Comp.OldDamage = damage.Damage;

        foreach (var (damageType, damageValue) in damage.Damage.DamageDict)
        {
            damage.Damage.DamageDict[damageType] = damageValue + ent.Comp.RegenerationModifier;
        }
    }

    private void OnShutdown(Entity<RegenerationGenComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<PassiveDamageComponent>(ent, out var damage))
            return;

        damage.Damage = ent.Comp.OldDamage;
    }
}
