// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Database;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects;

// Dumont start
public abstract partial class EntityEffect
{
    [DataField]
    public bool ScaleProbability;

    [DataField]
    public virtual float MinScale { get; private set; }

    [DataField]
    public virtual bool Scaling { get; private set; } = true;

    public virtual LogImpact? Impact => ShouldLog ? LogImpact : null;

    public virtual LogType LogType => LogType.Action;

    public virtual string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => ReagentEffectGuidebookText(prototype, entSys);

    public virtual void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user, bool predicted = true)
        => raiser.RaiseLegacyEffect(target, this, scale, user, predicted);
}

public abstract partial class EntityEffectBase<T> : EntityEffect where T : EntityEffectBase<T>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => EntityEffectGuidebookText(prototype, entSys);

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;

    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user, bool predicted = true)
    {
        if (this is T effect)
            raiser.RaiseEffectEvent(target, effect, scale, user, predicted);
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var scale = args is EntityEffectReagentArgs reagent ? reagent.Quantity.Float() * reagent.Scale.Float() : 1f;
        var context = args as EntityEffectUserArgs;
        RaiseEvent(args.TargetEntity, args.EntityManager.System<SharedEntityEffectsSystem>(), scale, context?.User, context?.Predicted ?? false);
    }
}

public sealed record EntityEffectUserArgs(EntityUid Target, IEntityManager Manager, float EffectScale, EntityUid? User, bool Predicted)
    : EntityEffectReagentArgs(Target, Manager, null, null, FixedPoint2.New(EffectScale), null, null, FixedPoint2.New(1));

public sealed partial class SharedEntityEffectsSystem
{
    public void RaiseLegacyEffect(EntityUid target, EntityEffect effect, float scale, EntityUid? user, bool predicted)
    {
        var args = new EntityEffectUserArgs(target, EntityManager, scale, user, predicted);
        EntityManager.System<SharedEntityEffectSystem>().Effect(effect, args);
    }

    private bool TryConditions(EntityUid target, EntityEffectCondition[]? conditions, EntityUid? user)
    {
        if (conditions == null)
            return true;

        var args = new EntityEffectUserArgs(target, EntityManager, 1f, user, true);
        foreach (var condition in conditions)
        {
            if (!condition.Condition(args))
                return false;
        }

        return true;
    }
}
// Dumont end
