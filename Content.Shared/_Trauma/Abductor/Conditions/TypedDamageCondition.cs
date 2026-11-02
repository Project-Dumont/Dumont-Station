// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to have at least this much of every listed damage type.
/// </summary>
public sealed partial class TypedDamageCondition : AbductorTaskCondition
{
    [DataField(required: true)]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> Damage = new();

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent<DamageableComponent>(target, out var damageable))
            return false;

        foreach (var (type, amount) in Damage)
        {
            if (!damageable.Damage.DamageDict.TryGetValue(type, out var current) || current < amount)
                return false;
        }

        return true;
    }
}
