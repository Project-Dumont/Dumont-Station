// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject's vital damage to be within some bounds.
/// </summary>
public sealed partial class VitalDamageCondition : AbductorTaskCondition
{
    [DataField]
    public FixedPoint2 Min;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent<DamageableComponent>(target, out var damageable))
            return false;

        var vital = entMan.System<MobThresholdSystem>().CheckVitalDamage(target, damageable);
        return vital >= Min && vital <= Max;
    }
}
