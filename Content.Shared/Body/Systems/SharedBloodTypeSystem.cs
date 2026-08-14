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
        catch (InvalidOperationException)
        {
            return "";
        }
    }

    public ProtoId<BloodTypePrototype>? GetBloodType(EntityUid uid)
    {
        if(!TryComp(uid, out BloodTypeComponent? comp))
            return null;
        return comp.Type;
    }

    public void SetBloodType(EntityUid uid, BloodTypeComponent? uidBloodType = null, HumanoidAppearanceComponent? uidAppearence = null)
    {
        if (!Resolve(uid, ref uidBloodType, ref uidAppearence, false))
            return;
        uidBloodType.Type = GenerateBloodType(uidAppearence);
        DirtyField(uid, uidBloodType, nameof(BloodTypeComponent.Type));
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
        if (foreignAmount != 0)
        {
            var internalBlood = Solution.Contents.SkipWhile(content => bloodstream.BloodReagent.Id == content.Reagent.ToString()).ToList();
            internalBlood.ForEach(content => Solution.RemoveReagent(content.Reagent, uidBloodType.ForeignBloodDeducted));
            damage = damage * 10f * (foreignAmount / bloodstream.BloodMaxVolume);
            _damageableSystem.TryChangeDamage(uid, damage, ignoreResistances: false,
            interruptsDoAfters: false, splitDamage: SplitDamageBehavior.SplitEnsureAll,
            targetPart: TargetBodyPart.All);
        }
    }
    public FixedPoint2 GetForeignBloodAmount(EntityUid uid)
    {
        FixedPoint2 amount = 0;
        if (!TryComp(uid,out BloodTypeComponent? bloodTypeComp ) || !TryComp<BloodstreamComponent>(uid,out var bloodstream))
            return amount;
        if( !SolutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution,out var Solution))
            return amount;
        foreach (var internalContent in Solution!.Contents)
        {
            foreach ( var data in internalContent.Reagent.EnsureReagentData())
            {
                if(data is BloodTypeData)
                {
                    if(bloodTypeComp.Type == ((BloodTypeData) data).Type)
                        continue;
                    amount += internalContent.Quantity;
                }
            }
        }
        return amount;
    }
}
