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
                ? ReverseSpoilage(solution, rate)
                : ProgressSpoilage(spoiling, solution);

            var stage = CalculateStage(solution);
            if (stage != spoiling.Stage)
            {
                spoiling.Stage = stage;
                Dirty(uid, spoiling);
            }

            if (changed)
                _solutionContainer.UpdateChemicals(solnEnt.Value);

            if (!HasSpoilableReagent(solution) && !HasReversibleReagent(solution))
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

            var spoiledId = new ReagentId(SpoiledReagentId, new List<ReagentData> { new SpoiledReagentData(reagent.Prototype) });
            solution.AddReagent(spoiledId, removed);
        }

        return true;
    }

    private bool ReverseSpoilage(Solution solution, float rate)
    {
        if (rate <= 0f)
            return false;

        var remaining = FixedPoint2.New(rate);
        if (remaining <= FixedPoint2.Zero)
            return false;

        var toRevert = new List<(ReagentId Reagent, ProtoId<ReagentPrototype> Original, FixedPoint2 Amount)>();
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (remaining <= FixedPoint2.Zero)
                break;

            if (!TryGetSpoiledOrigin(reagent, out var original))
                continue;

            var amount = FixedPoint2.Min(quantity, remaining);
            toRevert.Add((reagent, original, amount));
            remaining -= amount;
        }

        if (toRevert.Count == 0)
            return false;

        foreach (var (reagent, original, amount) in toRevert)
        {
            var removed = solution.RemoveReagent(reagent, amount);
            if (removed <= FixedPoint2.Zero)
                continue;

            solution.AddReagent(new ReagentId(original, null), removed);
        }

        return true;
    }

    private int CalculateStage(Solution solution)
    {
        var spoiled = FixedPoint2.Zero;
        var fresh = FixedPoint2.Zero;

        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (TryGetSpoiledOrigin(reagent, out _))
                spoiled += quantity;
            else if (IsSpoilable(reagent.Prototype))
                fresh += quantity;
        }

        var total = spoiled + fresh;
        if (total <= FixedPoint2.Zero)
            return 0;

        var stage = (int) (MaxStages * spoiled.Float() / total.Float());
        return Math.Clamp(stage, 0, MaxStages);
    }
}
