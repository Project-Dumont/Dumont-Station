using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphLongRangeSightSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedContentEyeSystem _eye = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, XenomorphLongRangeSightActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, XenomorphLongRangeSightDoAfterEvent>(OnPrepared);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, GetEyeOffsetEvent>(OnOffset);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphLongRangeSightComponent, XenomorphRestActionEvent>(OnRest, after: new[] { typeof(XenomorphRestSystem) });
    }

    private void OnAction(Entity<XenomorphLongRangeSightComponent> ent, ref XenomorphLongRangeSightActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting) return;
        if (ent.Comp.Enabled)
        {
            SetEnabled(ent, false);
            args.Handled = true;
            return;
        }
        if (ent.Comp.PrepareDelay <= TimeSpan.Zero)
        {
            SetEnabled(ent, true);
            args.Handled = true;
            return;
        }
        ent.Comp.Preparing = true;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.PrepareDelay,
            new XenomorphLongRangeSightDoAfterEvent(), ent) { BreakOnMove = true, NeedHand = false });
        if (!args.Handled) ent.Comp.Preparing = false;
    }

    private void OnPrepared(Entity<XenomorphLongRangeSightComponent> ent, ref XenomorphLongRangeSightDoAfterEvent args)
    {
        ent.Comp.Preparing = false;
        if (args.Handled || args.Cancelled || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting) return;
        args.Handled = true;
        SetEnabled(ent, true);
    }

    private void SetEnabled(Entity<XenomorphLongRangeSightComponent> ent, bool enabled)
    {
        ent.Comp.Enabled = enabled;
        ent.Comp.Offset = enabled ? Transform(ent).LocalRotation.GetCardinalDir().ToVec() * ent.Comp.OffsetLength : Vector2.Zero;
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphLongRangeSightActionEvent) _actions.SetToggled(action.Owner, enabled);
        Refresh(ent);
    }

    private void Refresh(Entity<XenomorphLongRangeSightComponent> ent)
    {
        _movement.RefreshMovementSpeedModifiers(ent);
        _eye.SetMaxZoom(ent.Owner, ent.Comp.Enabled ? ent.Comp.Zoom : SharedContentEyeSystem.DefaultZoom);
        _eye.SetZoom(ent.Owner, ent.Comp.Enabled ? ent.Comp.Zoom : SharedContentEyeSystem.DefaultZoom);
        _eye.UpdatePvsScale(ent.Owner);
        if (TryComp<EyeComponent>(ent, out var eye)) _eye.UpdateEyeOffset((ent, eye));
    }

    private void OnSpeed(Entity<XenomorphLongRangeSightComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Enabled) args.ModifySpeed(ent.Comp.Speed, ent.Comp.Speed);
    }

    private void OnOffset(Entity<XenomorphLongRangeSightComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Comp.Enabled) args.Offset += ent.Comp.Offset;
    }

    private void OnState(Entity<XenomorphLongRangeSightComponent> ent, ref AfterAutoHandleStateEvent args) => Refresh(ent);
    private void OnShutdown(Entity<XenomorphLongRangeSightComponent> ent, ref ComponentShutdown args) => SetEnabled(ent, false);
    private void OnMobState(Entity<XenomorphLongRangeSightComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive && ent.Comp.Enabled) SetEnabled(ent, false);
    }
    private void OnRest(Entity<XenomorphLongRangeSightComponent> ent, ref XenomorphRestActionEvent args)
    {
        if (ent.Comp.Enabled && TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting) SetEnabled(ent, false);
    }
}
