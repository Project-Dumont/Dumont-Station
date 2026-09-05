// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Gabystation.CCVar;
using Content.Shared._Gabystation.ChemicalSpoilage;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.ChemicalSpoilage;

/// <summary>
/// Server-side loop that progresses (or reverses) chemical spoilage on entities with a
/// SpoilingSolutionComponent.
/// </summary>
public sealed partial class ChemicalSpoilageSystem : SharedChemicalSpoilageSystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private const float MaxDesaturation = 0.35f;
    private TimeSpan _spoilageTime = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, GabyCVars.ChemSpoilageTime, v => _spoilageTime = v, true);
    }

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

            var stage = CalculateStage(solution);
            var stageChanged = stage != spoiling.Stage;
            if (stageChanged)
            {
                spoiling.Stage = stage;
                Dirty(uid, spoiling);
            }

            if (changed)
                _solutionContainer.UpdateChemicals(solnEnt.Value);

            if (changed || stageChanged)
                UpdateColor(uid, spoiling, solution);

            if (!HasSpoilableReagent(solution) && !HasReversibleReagent(solution))
                RemCompDeferred<SpoilingSolutionComponent>(uid);
        }
    }

    private bool ProgressSpoilage(SpoilingSolutionComponent spoiling, Solution solution)
    {
        if (spoiling.ShelfLife <= TimeSpan.Zero)
            return false;

        var decayRate = _spoilageTime <= TimeSpan.Zero ? 0f : (float) (20f / _spoilageTime.TotalMinutes);
        spoiling.SpoilAccumulator += spoiling.UpdateRate * decayRate;

        var elapsedFraction = Math.Clamp(
            (float) (spoiling.SpoilAccumulator.TotalSeconds / spoiling.ShelfLife.TotalSeconds), 0f, 1f);

        // Nothing actually spoils until we'd be entering stage 1 - a container that still "looks
        // fresh" (stage 0) shouldn't already be quietly poisoning anyone.
        if (elapsedFraction < 1f / MaxStages)
            return false;

        var totals = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();
        var alreadySpoiled = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (IsSpoilable(reagent.Prototype))
            {
                totals[reagent.Prototype] = totals.GetValueOrDefault(reagent.Prototype) + quantity;
            }
            else if (TryGetSpoiledOrigin(reagent, out var original))
            {
                totals[original] = totals.GetValueOrDefault(original) + quantity;
                alreadySpoiled[original] = alreadySpoiled.GetValueOrDefault(original) + quantity;
            }
        }

        if (totals.Count == 0)
            return false;

        var toConvert = new List<(ReagentId Reagent, FixedPoint2 Amount)>();
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (!IsSpoilable(reagent.Prototype))
                continue;

            var target = totals[reagent.Prototype] * elapsedFraction;
            var already = alreadySpoiled.GetValueOrDefault(reagent.Prototype);
            var delta = FixedPoint2.Min(quantity, target - already);
            if (delta > FixedPoint2.Zero)
                toConvert.Add((reagent, delta));
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

    private bool ReverseSpoilage(SpoilingSolutionComponent spoiling, Solution solution, float rate)
    {
        var reduced = spoiling.SpoilAccumulator - spoiling.UpdateRate;
        spoiling.SpoilAccumulator = reduced > TimeSpan.Zero ? reduced : TimeSpan.Zero;

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

    /// <summary>
    /// Changes the color of the reagent on how much spoiled it is.
    /// </summary>
    private void UpdateColor(EntityUid uid, SpoilingSolutionComponent spoiling, Solution solution)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        // Blends using each reagent original color
        var color = GetOriginalColor(solution);
        var amount = MaxDesaturation * (spoiling.Stage / (float) MaxStages);
        if (amount > 0f)
        {
            var luminance = color.R * 0.299f + color.G * 0.587f + color.B * 0.114f;
            var gray = new Color(luminance, luminance, luminance, color.A);
            color = Color.InterpolateBetween(color, gray, amount);
        }

        _appearance.SetData(uid, SolutionContainerVisuals.Color, color, appearance);
    }

    /// <summary>
    /// Returns the original color without the toxine reagent color.
    /// </summary>
    private Color GetOriginalColor(Solution solution)
    {
        if (solution.Volume == FixedPoint2.Zero)
            return Color.Transparent;

        Color mixColor = default;
        var runningTotalQuantity = FixedPoint2.Zero;
        var first = true;

        foreach (var (reagent, quantity) in solution.Contents)
        {
            var protoId = TryGetSpoiledOrigin(reagent, out var original)
                ? original
                : (ProtoId<ReagentPrototype>) reagent.Prototype;
            if (!Proto.TryIndex(protoId, out ReagentPrototype? proto))
                continue;

            runningTotalQuantity += quantity;

            if (first)
            {
                first = false;
                mixColor = proto.SubstanceColor;
                continue;
            }

            var interpolateValue = quantity.Float() / runningTotalQuantity.Float();
            mixColor = Color.InterpolateBetween(mixColor, proto.SubstanceColor, interpolateValue);
        }

        return mixColor;
    }
}
