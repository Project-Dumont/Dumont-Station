// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Damage;

// Dumont start
[ByRefEvent]
public record struct DamageDealtEvent(DamageSpecifier Damage, EntityUid? Origin, bool InterruptsDoAfters,
    bool IgnoreBlockers, DamageSpecifier ModifiedDamage);

public sealed partial class DamageableSystem
{
    public void SetAllDamage(Entity<DamageableComponent?> ent, Content.Goobstation.Maths.FixedPoint.FixedPoint2 value)
    {
        if (Resolve(ent, ref ent.Comp, false))
            SetAllDamage(ent.Owner, ent.Comp, value);
    }

    public void ClearAllDamage(EntityUid uid) => SetAllDamage((uid, null), 0);

    public DamageSpecifier GetAllDamage(EntityUid uid)
    {
        if (TryComp<Content.Shared.Body.Components.BodyComponent>(uid, out var body) &&
            body.BodyType == Content.Shared._Shitmed.Body.BodyType.Complex)
        {
            var result = new DamageSpecifier();
            foreach (var (_, _, damageable) in _body.GetBodyChildrenWithComponent<DamageableComponent>(uid))
                result += damageable.Damage;
            return result;
        }
        return TryComp<DamageableComponent>(uid, out var damage) ? new DamageSpecifier(damage.Damage) : new DamageSpecifier();
    }

    public DamageSpecifier ChangeDamage(Entity<DamageableComponent?> ent,
        DamageSpecifier damage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        EntityUid? origin = null,
        bool canBeCancelled = false,
        float partMultiplier = 1f,
        TargetBodyPart? targetPart = null,
        bool ignoreBlockers = false,
        SplitDamageBehavior splitDamage = SplitDamageBehavior.Split,
        bool canMiss = true)
    {
        return TryChangeDamage(ent.Owner, damage, ignoreResistances, interruptsDoAfters, ent.Comp, origin,
            canBeCancelled, partMultiplier, targetPart, ignoreBlockers, splitDamage, canMiss) ?? new DamageSpecifier();
    }
}
// Dumont end
