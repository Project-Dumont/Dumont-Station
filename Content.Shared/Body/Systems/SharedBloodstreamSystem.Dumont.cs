// Dumont start
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodstreamSystem
{
    public bool FlushChemicals(Entity<BloodstreamComponent?> ent, float quantity, IReadOnlyCollection<ProtoId<ReagentPrototype>> excluded)
    {
        if (!Resolve(ent, ref ent.Comp, logMissing: false) ||
            !SolutionContainer.ResolveSolution(ent.Owner, ent.Comp.ChemicalSolutionName, ref ent.Comp.ChemicalSolution, out var solution))
            return false;

        for (var i = solution.Contents.Count - 1; i >= 0; i--)
        {
            var reagent = solution.Contents[i].Reagent;
            if (!excluded.Contains(reagent.Prototype))
                SolutionContainer.RemoveReagent(ent.Comp.ChemicalSolution.Value, reagent, FixedPoint2.New(quantity));
        }
        return true;
    }
}
// Dumont end
