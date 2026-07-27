using System;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DeepFryer.Components;
using Content.Shared.DeepFryer.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeepFryer;

public sealed class DeepFryerSystem : SharedDeepFryerSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private ProtoId<DamageTypePrototype> damageType = "Heat";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<DeepFryerComponent>();
        while (query.MoveNext(out var fryerUid, out var fryer))
        {
            if (!fryer.Closed)
                continue;

            AddHeatToSolution((fryerUid, fryer), frameTime, fryer.HeatToAddToSolution);

            if (fryer.StoredObjects.Count == 0)
                continue;

            AddHeatDamage((fryerUid, fryer), frameTime);

            if (fryer.FryFinishTime < _timing.CurTime && fryer.FryFinishTime != TimeSpan.Zero)
            {
                DeepFryItems((fryerUid,fryer));
            }
        }
    }

    private void AddHeatToSolution(Entity<DeepFryerComponent> ent, float frameTime, float heatToAdd)
    {
        if (_solution.TryGetSolution(ent.Owner,
                ent.Comp.FryerSolutionContainer,
                out var solutionRef,
                out var solution)) // Dumont
        {
            solution.Temperature = Math.Clamp(solution.Temperature + (heatToAdd * frameTime), 293f, ent.Comp.MaxHeat);

            
            _solution.UpdateChemicals(solutionRef.Value);
            // Dumont End
        }
    }

    private void AddHeatDamage(Entity<DeepFryerComponent> ent, float frameTime)
    {
        var heatProto = _prototypeManager.Index(damageType);

        foreach (var entity in ent.Comp.StoredObjects)
        {
            if (!TryComp<DamageableComponent>(entity, out _))
                continue;

            _damageable.TryChangeDamage(entity, new DamageSpecifier(heatProto, ent.Comp.HeatDamage * frameTime));
        }
    }
}