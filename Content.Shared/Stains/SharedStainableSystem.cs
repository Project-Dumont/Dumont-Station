using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Slippery;
using Content.Shared.WashingMachine.Events;

namespace Content.Shared.Stains;

public abstract partial class SharedStainableSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem Solution = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StainableComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<StainableComponent, InventoryRelayedEvent<SlippedEvent>>(OnSlipped);
        SubscribeLocalEvent<StainableComponent, InventoryRelayedEvent<SpilledOnEvent>>(OnSpilledOn);

        SubscribeLocalEvent<StainableComponent, WashingMachineIsBeingWashed>(OnWashed);
    }

    private void OnInit(Entity<StainableComponent> ent, ref ComponentInit args)
    {
        if (!Solution.EnsureSolution(ent.Owner, ent.Comp.SolutionId, out var solution, ent.Comp.MaxVolume))
            return;

        solution.CanReact = false;
        UpdateVisuals(ent);
    }

    private void OnSlipped(Entity<StainableComponent> ent, ref InventoryRelayedEvent<SlippedEvent> args)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var target))
            return;

        var ev = new GetStainableSolutionEvent(ent.Owner);
        RaiseLocalEvent(args.Args.Slipper, ev);

        if (!ev.Handled || ev.Solution == null)
            return;

        Solution.TryTransferSolution(target.Value, ev.Solution, ent.Comp.StainVolume);

        UpdateVisuals(ent);
        StainForensics(ent, target.Value);
    }

    private void OnSpilledOn(Entity<StainableComponent> ent, ref InventoryRelayedEvent<SpilledOnEvent> args)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var target))
            return;

        Solution.TryTransferSolution(target.Value, args.Args.Solution, ent.Comp.StainVolume);

        UpdateVisuals(ent);
        StainForensics(ent, target.Value);
    }

    private void OnWashed(Entity<StainableComponent> ent, ref WashingMachineIsBeingWashed args)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var solution))
            return;

        WashingForensics(ent, solution.Value, args.WashingMachine);
        Solution.RemoveAllSolution(solution.Value);
        UpdateVisuals(ent);
    }

    protected virtual void StainForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution)
    {
    }

    protected virtual void WashingForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution, EntityUid washingMachine)
    {
    }

    private void UpdateVisuals(Entity<StainableComponent> ent)
    {
        _item.VisualsChanged(ent.Owner);

        // there isnt a value to parse as its calculated on every change
        // so just do a blanket update and calculate on the client
        if (TryComp<AppearanceComponent>(ent.Owner, out var appearance))
            _appearance.QueueUpdate(ent.Owner, appearance);
    }
}
