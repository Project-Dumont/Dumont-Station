using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Genetics;

namespace Content.Server.Genetics.System;

public sealed class RegenerationGenSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;

    [ValidatePrototypeId<DamageTypePrototype>]
    private const string BluntDamage = "Blunt";
    [ValidatePrototypeId<DamageTypePrototype>]
    private const string HeatDamage = "Heat";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var regenerationQuery = EntityQueryEnumerator<RegenerationGenComponent>();
        while (regenerationQuery.MoveNext(out var uid, out var regenerationComponent))
        {
            if (regenerationComponent.NextTimeTick <= 0)
            {
                regenerationComponent.NextTimeTick = 4f;
                if (!TryComp<DamageableComponent>(uid, out var damageable))
                    return;

                var modifier = regenerationComponent.RegenerationModifier;
                var damage = new DamageSpecifier { DamageDict = { { BluntDamage, modifier }, { HeatDamage, modifier } } };
                _damage.TryChangeDamage(uid, damage, true, damageable: damageable);
            }
            regenerationComponent.NextTimeTick -= frameTime;
        }
    }
}
