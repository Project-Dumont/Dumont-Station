// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Gabystation.ChemicalSpoilage;

/// <summary>
/// Handles the entire medical chems rotting proccess.
/// </summary>
public abstract class SharedChemicalSpoilageSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;

    public const int MaxStages = 3;

    /// <summary>
    /// Only chems with fields in this group are able to spoil
    /// </summary>
    public static readonly ProtoId<MetabolismGroupPrototype> MedicineGroup = "Medicine";

    /// <summary>
    /// The reagent spoiled medicine turns into. Tagged with SpoiledReagentData so it can be told
    /// apart from Toxin someone actually injected on purpose, and so it can be reverted back.
    /// </summary>
    public static readonly ProtoId<ReagentPrototype> SpoiledReagentId = "Toxin";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpoilingSolutionComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SolutionComponent, SolutionChangedEvent>(OnSolutionChanged);
    }

    private void OnExamined(Entity<SpoilingSolutionComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || !HasComp<ExaminableSolutionComponent>(ent))
            return;

        if (ent.Comp.Stage < 1 || ent.Comp.Stage > MaxStages)
            return;

        var msg = Loc.GetString("chem-spoilage-flavour-stage-" + ent.Comp.Stage,
            ("target", Identity.Entity(ent, EntityManager)));
        args.PushMarkup(msg);
    }

    private void OnSolutionChanged(Entity<SolutionComponent> ent, ref SolutionChangedEvent args)
    {
        if (!TryComp<ContainedSolutionComponent>(ent, out var contained))
            return;

        if (HasComp<SpoilingSolutionComponent>(contained.Container))
            return;

        if (!HasSpoilableReagent(ent.Comp.Solution) && !HasReversibleReagent(ent.Comp.Solution))
            return;

        var spoiling = EnsureComp<SpoilingSolutionComponent>(contained.Container);
        spoiling.Solution = contained.ContainerName;
        spoiling.NextUpdate = Timing.CurTime + spoiling.UpdateRate;
    }

    public bool IsSpoilable(ProtoId<ReagentPrototype> reagent)
    {
        return Proto.TryIndex(reagent, out var proto) && IsSpoilable(proto);
    }

    public bool IsSpoilable(ReagentPrototype proto)
    {
        return proto.Metabolisms?.ContainsKey(MedicineGroup) == true;
    }

    public bool HasSpoilableReagent(Solution solution)
    {
        foreach (var (reagent, _) in solution.Contents)
        {
            if (IsSpoilable(reagent.Prototype))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether this solution still has any already-spoiled reagent in it.
    /// </summary>
    public bool HasReversibleReagent(Solution solution)
    {
        foreach (var (reagent, _) in solution.Contents)
        {
            if (TryGetSpoiledOrigin(reagent, out _))
                return true;
        }

        return false;
    }

    /// <summary>
    /// If this reagent is spoiled medicine tagged with SpoiledReagentData, returns the reagent it
    /// originally was before spoiling.
    /// </summary>
    public static bool TryGetSpoiledOrigin(ReagentId reagent, out ProtoId<ReagentPrototype> original)
    {
        original = default;

        if (reagent.Prototype != SpoiledReagentId || reagent.Data is null)
            return false;

        foreach (var data in reagent.Data)
        {
            if (data is not SpoiledReagentData spoiled)
                continue;

            original = spoiled.OriginalReagent;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the reversal rate of whatever ReagentPreserverComponent is protecting this container
    /// </summary>
    public float? GetPreservationRate(EntityUid uid)
    {
        if (TryComp<ReagentPreserverComponent>(uid, out var self))
            return self.ReversalRate;

        if (Container.TryGetContainingContainer((uid, null, null), out var container) &&
            TryComp<ReagentPreserverComponent>(container.Owner, out var outer))
        {
            return outer.ReversalRate;
        }

        return null;
    }
}
