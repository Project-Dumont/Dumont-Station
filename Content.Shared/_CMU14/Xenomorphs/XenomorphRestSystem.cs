using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphRestSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRestComponent, XenomorphRestActionEvent>(OnRest);
        SubscribeLocalEvent<XenomorphRestComponent, UpdateCanMoveEvent>(OnCanMove);
        SubscribeLocalEvent<XenomorphRestComponent, AttackAttemptEvent>(OnAttack);
        SubscribeLocalEvent<XenomorphRestComponent, InteractionAttemptEvent>(OnInteract);
        SubscribeLocalEvent<XenomorphRestComponent, MobStateChangedEvent>(OnMobState);
    }

    private void OnRest(Entity<XenomorphRestComponent> ent, ref XenomorphRestActionEvent args)
    {
        if (args.Handled || !_mobState.IsAlive(ent)
            || TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered
            || TryComp<XenomorphFortifyComponent>(ent, out var fortify) && fortify.Fortified
            || TryComp<XenomorphOvipositorComponent>(ent, out var ovi) && ovi.Attached)
            return;

        ent.Comp.RestAction = args.Action.Owner;
        SetResting(ent, !ent.Comp.Resting);

        args.Handled = true;
    }

    private void OnCanMove(Entity<XenomorphRestComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Resting)
            args.Cancel();
    }

    private void OnAttack(Entity<XenomorphRestComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Resting)
            args.Cancel();
    }

    private void OnInteract(Entity<XenomorphRestComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.Resting)
            args.Cancelled = true;
    }

    private void OnMobState(Entity<XenomorphRestComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            SetResting(ent, false);
    }

    private void SetResting(Entity<XenomorphRestComponent> ent, bool resting)
    {
        ent.Comp.Resting = resting;
        _actions.SetToggled(ent.Comp.RestAction, resting);
        Dirty(ent);
        _appearance.SetData(ent, XenomorphVisuals.Resting, resting);
        _blocker.UpdateCanMove(ent);
    }
}
