using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public abstract partial class SharedXenomorphChargeSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private PullingSystem _pulling = default!;
    private static readonly SoundSpecifier Roar = new SoundCollectionSpecifier("DumontXenomorphRoar", AudioParams.Default.WithVariation(0.125f));
    private static readonly SoundSpecifier Footstep = new SoundCollectionSpecifier("XenoFootstepLarge", AudioParams.Default.WithVolume(-2));
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPlasmaSystem _plasma = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphChargeComponent, XenomorphChargeActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphChargeComponent, XenomorphChargeModeActionEvent>(OnMode);
        SubscribeLocalEvent<XenomorphChargeComponent, MoveInputEvent>(OnInput);
        SubscribeLocalEvent<XenomorphChargeComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<XenomorphChargeComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphChargeComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphChargeComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnState(Entity<XenomorphChargeComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnSpeed(Entity<XenomorphChargeComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
        {
            var modifier = 1 + ent.Comp.Stage * ent.Comp.SpeedPerStage;
            args.ModifySpeed(modifier, modifier);
        }
    }

    private void OnAction(Entity<XenomorphChargeComponent> ent, ref XenomorphChargeActionEvent args)
    {
        if (_net.IsClient || args.Handled || !_mobState.IsAlive(ent))
            return;
        ent.Comp.Action = args.Action.Owner;
        ent.Comp.Active = !ent.Comp.Active;
        Reset(ent);
        _actions.SetToggled(args.Action.Owner, ent.Comp.Active);
        args.Handled = true;
    }

    private void OnMode(Entity<XenomorphChargeComponent> ent, ref XenomorphChargeModeActionEvent args)
    {
        if (_net.IsClient || args.Handled || !_mobState.IsAlive(ent))
            return;
        ent.Comp.Mode = args.Mode;
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(action) is XenomorphChargeModeActionEvent mode)
                _actions.SetToggled(action.Owner, mode.Mode == args.Mode);
        }
        args.Handled = true;
    }

    private void OnInput(Entity<XenomorphChargeComponent> ent, ref MoveInputEvent args)
    {
        if (_net.IsClient || !ent.Comp.Active)
            return;
        var direction = args.Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection;
        if (direction != ent.Comp.Direction)
            Reset(ent);
        ent.Comp.Direction = direction;
    }

    private void OnMove(Entity<XenomorphChargeComponent> ent, ref MoveEvent args)
    {
        if (_net.IsClient || !ent.Comp.Active || !CanCharge(ent)
            || !TryComp<InputMoverComponent>(ent, out var mover))
            return;
        var direction = mover.HeldMoveButtons & MoveButtons.AnyDirection;
        if (direction == MoveButtons.None || ((byte) direction & ((byte) direction - 1)) != 0)
        {
            Reset(ent);
            return;
        }
        if (direction != ent.Comp.Direction || mover.RelativeRotation != ent.Comp.RelativeRotation
            || ent.Comp.MovementParent != args.NewPosition.EntityId)
        {
            Reset(ent);
            ent.Comp.Direction = direction;
            ent.Comp.RelativeRotation = mover.RelativeRotation;
            ent.Comp.MovementParent = args.NewPosition.EntityId;
        }
        if (args.OldPosition.EntityId != args.NewPosition.EntityId)
            return;
        var distance = (args.NewPosition.Position - args.OldPosition.Position).Length();
        if (distance > 2)
        {
            Reset(ent);
            return;
        }
        if (distance <= 0.001f)
            return;
        ent.Comp.LastMovedAt = _timing.CurTime;
        ent.Comp.Distance += distance;
        while (ent.Comp.Distance >= ent.Comp.StepDistance)
        {
            ent.Comp.Distance -= ent.Comp.StepDistance;
            ent.Comp.Steps++;
            if (ent.Comp.Steps < ent.Comp.MinimumSteps)
                continue;
            if (!_plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaPerStep))
            {
                Reset(ent);
                return;
            }
            if (ent.Comp.Stage == ent.Comp.MaxStage - 1 && _timing.CurTime >= ent.Comp.NextRoar)
            {
                _audio.PlayPvs(Roar, ent);
                ent.Comp.NextRoar = _timing.CurTime + TimeSpan.FromSeconds(8);
            }
            ent.Comp.Stage = Math.Min(ent.Comp.MaxStage, ent.Comp.Stage + 1);
            if (TryComp<PullerComponent>(ent, out var puller) && puller.Pulling is { } victim
                && TryComp<PullableComponent>(victim, out var pullable))
                _pulling.TryStopPull(victim, pullable, ignoreGrab: true);
            if (ent.Comp.Stage == 1 || ent.Comp.Steps % 2 == 0)
                _audio.PlayPvs(Footstep, ent);
            Dirty(ent);
            _movement.RefreshMovementSpeedModifiers(ent);
        }
    }

    private bool CanCharge(Entity<XenomorphChargeComponent> ent)
    {
        return _mobState.IsAlive(ent) && _blocker.CanMove(ent) && !Transform(ent).Anchored
            && (!TryComp<PullableComponent>(ent, out var pulled) || !pulled.BeingPulled)
            && (!TryComp<PhysicsComponent>(ent, out var body) || body.BodyStatus != BodyStatus.InAir);
    }

    private void OnMobState(Entity<XenomorphChargeComponent> ent, ref MobStateChangedEvent args)
    {
        if (_net.IsClient || args.NewMobState == MobState.Alive)
            return;
        ent.Comp.Active = false;
        _actions.SetToggled(ent.Comp.Action, false);
        Reset(ent);
    }

    public void Reset(Entity<XenomorphChargeComponent> ent)
    {
        ent.Comp.Stage = 0;
        ent.Comp.Steps = 0;
        ent.Comp.Distance = 0;
        ent.Comp.Direction = MoveButtons.None;
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_net.IsClient)
            return;
        var query = EntityQueryEnumerator<XenomorphChargeComponent>();
        while (query.MoveNext(out var uid, out var charge))
        {
            if (charge.Active && charge.Steps > 0
                && (!CanCharge((uid, charge)) || _timing.CurTime - charge.LastMovedAt > charge.MoveGrace))
                Reset((uid, charge));
        }
    }
}
