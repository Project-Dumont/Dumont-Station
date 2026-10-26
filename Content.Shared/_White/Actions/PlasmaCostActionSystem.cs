using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Actions;
using Content.Shared._White.Actions.Events;
using Content.Shared.Actions.Events;

namespace Content.Shared._White.Actions;

public sealed partial class PlasmaCostActionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPlasmaSystem _plasma = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionRelayedEvent<PlasmaAmountChangeEvent>>(OnPlasmaAmountChange);
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionAttemptEvent>(OnActionAttempt); // Goobstation
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionPerformedEvent>(OnActionPerformed); // Dumont
    }

    /// <summary>
    /// Checks if the performer has enough plasma for the action.
    /// Returns true if the action should proceed, false if it should be blocked.
    /// Goobstation
    /// </summary>
    public bool HasEnoughPlasma(EntityUid performer, FixedPoint2 cost)
    {
        if (cost <= 0)
            return true;

        return TryComp<PlasmaVesselComponent>(performer, out var plasmaVessel) &&
               plasmaVessel.Plasma >= cost;
    }

    /// <summary>
    /// Deducts plasma from the performer. Call this after confirming the action succeeds.
    /// </summary>
    public void DeductPlasma(EntityUid performer, FixedPoint2 cost)
    {
        if (cost > 0)
            _plasma.ChangePlasmaAmount(performer, -cost);
    }

    [Obsolete("Use HasEnoughPlasma and DeductPlasma separately for better control")]
    public bool CheckPlasmaCost(EntityUid performer, FixedPoint2 cost)
    {
        if (!HasEnoughPlasma(performer, cost))
            return false;

        DeductPlasma(performer, cost);
        return true;
    }

    private bool IsResinConstruction(EntityUid action)
        => _actions.GetEvent(action) is PlaceTileEntityEvent { Entity: { } resin } && (resin.Id == "WallResin" || resin.Id == "ResinDoor");

    private void OnPlasmaAmountChange(EntityUid uid, PlasmaCostActionComponent component, ActionRelayedEvent<PlasmaAmountChangeEvent> args)
    {
        _actions.SetEnabled(uid, (IsResinConstruction(uid) ? MathF.Ceiling(component.PlasmaCost.Float() / 2) : component.PlasmaCost) <= args.Args.Amount);
    }

    // Dumont start
    private void OnActionPerformed(Entity<PlasmaCostActionComponent> ent, ref ActionPerformedEvent args)
    {
        if (ent.Comp.ShouldChangePlasma)
            DeductPlasma(args.Performer, ent.Comp.PlasmaCost);
    }
    // Dumont end

    private void OnActionAttempt(Entity<PlasmaCostActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (!_plasma.HasPlasma(args.User, IsResinConstruction(ent) ? MathF.Ceiling(ent.Comp.PlasmaCost.Float() / 2) : ent.Comp.PlasmaCost))
            args.Cancelled = true;
    }
    // Goobstation end
}
