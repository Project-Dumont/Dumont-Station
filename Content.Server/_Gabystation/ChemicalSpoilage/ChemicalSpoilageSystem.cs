using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Gabystation.ChemicalSpoilage;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.ChemicalSpoilage;

/// <summary>
/// Server-side loop that progresses (or reverses) chemical spoilage on entities with a
/// SpoilingSolutionComponent.
/// </summary>
public sealed class ChemicalSpoilageSystem : SharedChemicalSpoilageSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    private static readonly ProtoId<ReagentPrototype> SpoiledReagent = "Toxin";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpoilingSolutionComponent>();
        while (query.MoveNext(out var uid, out var spoiling))
        {
            if (Timing.CurTime < spoiling.NextUpdate)
                continue;
            spoiling.NextUpdate += spoiling.UpdateRate;

            if (!_solutionContainer.TryGetSolution((uid, null), spoiling.Solution, out var solnEnt, out var solution))
            {
                RemCompDeferred<SpoilingSolutionComponent>(uid);
                continue;
            }

            var preservationRate = GetPreservationRate(uid);
            var changed = preservationRate is { } rate
                ? ReverseSpoilage(spoiling, solution, rate)
                : ProgressSpoilage(spoiling, solution);

            var stage = CalculateStage(spoiling, solution);
            if (stage != spoiling.Stage)
            {
                spoiling.Stage = stage;
                Dirty(uid, spoiling);
            }

            if (changed)
                _solutionContainer.UpdateChemicals(solnEnt.Value);

            if (spoiling.Ledger.Count == 0 && !HasSpoilableReagent(solution))
                RemCompDeferred<SpoilingSolutionComponent>(uid);
        }
    }

    private bool ProgressSpoilage(SpoilingSolutionComponent spoiling, Solution solution)
    {
        var fraction = (float) (spoiling.UpdateRate.TotalSeconds / spoiling.ShelfLife.TotalSeconds);
        if (fraction <= 0f)
            return false;

        var toConvert = new List<(ReagentId Reagent, FixedPoint2 Amount)>();
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (!IsSpoilable(reagent.Prototype))
                continue;

            var amount = quantity * fraction;
            if (amount > FixedPoint2.Zero)
                toConvert.Add((reagent, amount));
        }

        if (toConvert.Count == 0)
            return false;

        foreach (var (reagent, amount) in toConvert)
        {
            var removed = solution.RemoveReagent(reagent, amount);
            if (removed <= FixedPoint2.Zero)
                continue;

            solution.AddReagent(SpoiledReagent, removed);
            AddToLedger(spoiling, reagent.Prototype, removed);
        }

        return true;
    }

    private bool ReverseSpoilage(SpoilingSolutionComponent spoiling, Solution solution, float rate)
    {
        if (spoiling.Ledger.Count == 0 || rate <= 0f)
            return false;

        var remaining = FixedPoint2.New(rate);
        var changed = false;

        for (var i = spoiling.Ledger.Count - 1; i >= 0 && remaining > FixedPoint2.Zero; i--)
        {
            var entry = spoiling.Ledger[i];
            var toRevert = FixedPoint2.Min(entry.Quantity, remaining);

            var removedToxin = solution.RemoveReagent(SpoiledReagent, toRevert);
            if (removedToxin <= FixedPoint2.Zero)
                break; // nothing left to revert

            solution.AddReagent(new ReagentId(entry.Reagent, null), removedToxin);
            entry.Quantity -= removedToxin;
            remaining -= removedToxin;
            changed = true;

            if (entry.Quantity <= FixedPoint2.Zero)
                spoiling.Ledger.RemoveAt(i);
        }

        return changed;
    }

    private static void AddToLedger(SpoilingSolutionComponent spoiling, ProtoId<ReagentPrototype> reagent, FixedPoint2 quantity)
    {
        foreach (var entry in spoiling.Ledger)
        {
            if (entry.Reagent != reagent)
                continue;

            entry.Quantity += quantity;
            return;
        }

        spoiling.Ledger.Add(new SpoiledPortion(reagent, quantity));
    }

    private int CalculateStage(SpoilingSolutionComponent spoiling, Solution solution)
    {
        var spoiled = FixedPoint2.Zero;
        foreach (var entry in spoiling.Ledger)
            spoiled += entry.Quantity;

        var fresh = FixedPoint2.Zero;
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (IsSpoilable(reagent.Prototype))
                fresh += quantity;
        }

        var total = spoiled + fresh;
        if (total <= FixedPoint2.Zero)
            return 0;

        var stage = (int) (MaxStages * spoiled.Float() / total.Float());
        return Math.Clamp(stage, 0, MaxStages);
    }
}
