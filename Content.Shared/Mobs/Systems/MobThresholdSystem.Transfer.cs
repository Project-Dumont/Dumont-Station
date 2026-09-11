using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Mobs.Systems;

public sealed partial class MobThresholdSystem
{
    public FixedPoint2 GetLowestThreshold(EntityUid target)
    {
        if (TryGetThresholdForState(target, MobState.Critical, out var threshold)
            || TryGetThresholdForState(target, MobState.Dead, out threshold))
            return threshold.Value;

        return FixedPoint2.Zero;
    }

    public void TransferDamage(EntityUid source, EntityUid target)
    {
        if (!TryComp<DamageableComponent>(target, out var damageable)
            || !GetScaledDamage(source, target, out var damage, out var parts)
            || damage == null)
            return;

        var damageSystem = EntityManager.System<DamageableSystem>();
        if (HasComp<BodyComponent>(target) && _body.TryGetRootPart(target, out var root))
        {
            var woundables = _wound.GetAllWoundableChildrenWithComp<DamageableComponent>(root.Value).ToArray();
            foreach (var part in woundables)
            {
                if (parts != null)
                {
                    var key = _body.GetTargetBodyPart(part);
                    damageSystem.SetDamage(part.Owner, part.Comp2,
                        parts.TryGetValue(key, out var partDamage) ? partDamage : new DamageSpecifier());
                }
                else if (woundables.Length > 0)
                {
                    damageSystem.SetDamage(part.Owner, part.Comp2, damage / woundables.Length);
                }
            }
        }

        damageSystem.SetDamage(target, damageable, damage);
    }
}
