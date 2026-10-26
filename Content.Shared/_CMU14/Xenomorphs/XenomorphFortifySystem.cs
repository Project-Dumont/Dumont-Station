using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphFortifySystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private FixtureSystem _fixtures = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private INetManager _net = default!;
    private const string FixtureId = "xenomorph-fortify";

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphFortifyComponent, XenomorphFortifyActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphFortifyComponent, UpdateCanMoveEvent>(OnCanMove);
        SubscribeLocalEvent<XenomorphFortifyComponent, ChangeDirectionAttemptEvent>(OnTurn);
        SubscribeLocalEvent<XenomorphFortifyComponent, AttackAttemptEvent>(OnAttack);
        SubscribeLocalEvent<XenomorphFortifyComponent, KnockDownAttemptEvent>(OnKnockdown);
        SubscribeLocalEvent<XenomorphFortifyComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphFortifyComponent, AnchorStateChangedEvent>(OnAnchor);
    }

    private void OnAction(Entity<XenomorphFortifyComponent> ent, ref XenomorphFortifyActionEvent args)
    {
        if (_net.IsClient || args.Handled || !_mobState.IsAlive(ent)
            || TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting)
            return;

        if (!ent.Comp.Fortified)
        {
            _transform.AnchorEntity(ent, Transform(ent));
            if (!Transform(ent).Anchored)
                return;
            _fixtures.TryCreateFixture(ent, new PhysShapeCircle(0.49f), FixtureId,
                hard: true, collisionLayer: (int) CollisionGroup.WallLayer);
        }
        ent.Comp.Action = args.Action.Owner;
        SetFortified(ent, !ent.Comp.Fortified);
        args.Handled = true;
    }

    private void OnCanMove(Entity<XenomorphFortifyComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Fortified)
            args.Cancel();
    }

    private void OnTurn(Entity<XenomorphFortifyComponent> ent, ref ChangeDirectionAttemptEvent args)
    {
        if (ent.Comp.Fortified)
            args.Cancel();
    }

    private void OnAttack(Entity<XenomorphFortifyComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Fortified && (args.Target == null || HasComp<MobStateComponent>(args.Target)))
            args.Cancel();
    }

    private void OnKnockdown(Entity<XenomorphFortifyComponent> ent, ref KnockDownAttemptEvent args)
    {
        if (ent.Comp.Fortified)
            args.Cancelled = true;
    }

    private void OnMobState(Entity<XenomorphFortifyComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_net.IsClient && ent.Comp.Fortified && args.NewMobState != MobState.Alive)
            SetFortified(ent, false);
    }

    private void OnAnchor(Entity<XenomorphFortifyComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!_net.IsClient && !args.Anchored && ent.Comp.Fortified)
            SetFortified(ent, false);
    }

    private void SetFortified(Entity<XenomorphFortifyComponent> ent, bool fortified)
    {
        ent.Comp.Fortified = fortified;
        if (!fortified)
        {
            _fixtures.DestroyFixture(ent, FixtureId);
            _transform.Unanchor(ent, Transform(ent));
            _physics.TrySetBodyType(ent, BodyType.KinematicController);
        }
        Dirty(ent);
        _armor.RefreshAlert(ent);
        _actions.SetToggled(ent.Comp.Action, fortified);
        _appearance.SetData(ent, XenomorphVisuals.Fortified, fortified);
        _blocker.UpdateCanMove(ent);
    }
}
