using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Body.Prototypes;
using Content.Shared.Body.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Humanoid;
using Content.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using Robust.Shared.Toolshed.Syntax;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodTypeSystem : EntitySystem
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem SolutionContainer = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public string GenerateBloodType(HumanoidAppearanceComponent? uidAppearence)
    {
        ProtoId<WeightedRandomPrototype> bloodTypesWeights = uidAppearence!.Species.Id + "Types";
        try
        {
            return SharedRandomExtensions.Pick(_prototypeManager.Index(bloodTypesWeights), _random);
        }
        catch (Exception e)
        {
            bloodTypesWeights = "HumanTypes";
            return SharedRandomExtensions.Pick(_prototypeManager.Index(bloodTypesWeights), _random);
        }
    }

    public ProtoId<BloodTypePrototype>? GetBloodType(EntityUid uid)
    {
        if (!TryComp(uid, out BloodTypeComponent? comp))
            return null;
        return comp.Type;
    }

    public void SetBloodType(EntityUid uid, BloodTypeComponent? uidBloodType = null, HumanoidAppearanceComponent? uidAppearence = null)
    {
        if (!Resolve(uid, ref uidBloodType))
            return;
        if (uidBloodType.Type is not null)
            return;
        if (!Resolve(uid, ref uidAppearence, false))
            return;
        uidBloodType.Type = GenerateBloodType(uidAppearence);
    }

    public void ApplyBloodTypeDamage (EntityUid uid, BloodTypeComponent? uidBloodType = null)
    {
        if (!Resolve(uid, ref uidBloodType))
            return;
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;
        if (_prototypeManager.Index(uidBloodType.Type) is null || uidBloodType.Type is null)
            return;
        if (!SolutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution,out var Solution))
            return;
        var damage = _prototypeManager!.Index(uidBloodType.Type).IncompatibilityDamage;
        FixedPoint2 foreignAmount = GetForeignBloodAmount(uid);
        if (foreignAmount > 0)
        {
            damage = damage * 10f * (foreignAmount / bloodstream.BloodMaxVolume);
            _damageableSystem.TryChangeDamage(uid, damage, ignoreResistances: false,
            interruptsDoAfters: false, splitDamage: SplitDamageBehavior.SplitEnsureAll,
            targetPart: TargetBodyPart.All);
            var internalBlood = GetForeignBloodList(uidBloodType, Solution);
            internalBlood.ForEach(content => Solution.RemoveReagent(content.Reagent, uidBloodType.ForeignBloodDeducted));
        }
    }

    public List<ReagentQuantity> GetForeignBloodList(BloodTypeComponent comp, Solution? soln)
    {
        List<ReagentQuantity> aux = new List<ReagentQuantity>();
        if (soln is null)
            return aux;
        var type = _prototypeManager!.Index(comp.Type);
        foreach (var content in soln.Contents)
        {
            foreach (var data in content.Reagent.EnsureReagentData())
            {
                if (data is BloodTypeData)
                    if (!type!.Compatibilities!.Contains(((BloodTypeData) data)?.Type ?? "N/A"))
                        aux.Add(content);
            }
        }
        return aux;
    }
    public FixedPoint2 GetForeignBloodAmount(EntityUid uid)
    {
        FixedPoint2 amount = 0;
        if (!TryComp(uid,out BloodTypeComponent? bloodTypeComp ) || !TryComp<BloodstreamComponent>(uid,out var bloodstream))
            return amount;
        if( !SolutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution,out var Solution))
            return amount;
        if(bloodTypeComp.Type is null)
            return amount;
        var type = _prototypeManager!.Index(bloodTypeComp.Type);
        foreach (var internalContent in Solution!.Contents)
        {
            foreach ( var data in internalContent.Reagent.EnsureReagentData())
            {
                if(data is BloodTypeData)
                {
                    if(type!.Compatibilities!.Contains(((BloodTypeData) data)?.Type ?? "N/A"))
                        continue;
                    amount += internalContent.Quantity;
                }
            }
        }
        return amount;
    }

    public void SetBloodData(EntityUid uid)
    {
        if (!TryComp(uid,out BloodstreamComponent? bloodstream))
            return;
        if (!TryComp(uid, out BloodTypeComponent? bloodTypeComp))
            return;
        if (!SolutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var soln))
            return;
        BloodTypeData typeData = new BloodTypeData();
        typeData.Type = bloodTypeComp.Type;
        foreach(var internalContent in soln!.Contents)
        {
            var data = internalContent.Reagent.EnsureReagentData();
            if(!data.Exists(aux => aux is BloodTypeData))
                data.Add(typeData);
        }

    }

    public bool IsBloodDataSet(Solution? soln)
    {
        bool aux = false;
        foreach (var internalContent in soln!.Contents)
        {
            var data = internalContent.Reagent.EnsureReagentData();
            if(!data.Exists(aux => aux is BloodTypeData) || data is null)
                continue;
            aux = true;
        }
        return aux;
    }
    public void SetBloodData(EntityUid uid,ProtoId<BloodTypePrototype>? type)
    {
        if (!TryComp(uid,out BloodstreamComponent? bloodstream) || type is null)
            return;
        if (!SolutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var soln))
            return;
        BloodTypeData typeData = new BloodTypeData();
        typeData.Type = type;
        foreach (var internalContent in soln!.Contents)
        {
            var data = internalContent.Reagent.EnsureReagentData();
            if(!data.Exists(aux => aux is BloodTypeData))
                data.Add(typeData);
        }
    }
}
