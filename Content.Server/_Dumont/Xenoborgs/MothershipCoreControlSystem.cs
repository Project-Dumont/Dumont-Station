// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Shared._Dumont.Xenoborgs;
using Content.Shared.Actions;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Robotics;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Xenoborgs.Components;

namespace Content.Server._Dumont.Xenoborgs;

public sealed partial class MothershipCoreControlSystem : EntitySystem
{
    [Dependency] private GhostRoleSystem _ghostRole = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MothershipCoreControlComponent, RoboticsConsoleTakeControlMessage>(OnTakeControl);
        SubscribeLocalEvent<MothershipCoreControlComponent, MindRemovedMessage>(OnCoreMindRemoved);

        SubscribeLocalEvent<MothershipControlledComponent, ReturnToMothershipCoreEvent>(OnReturn);
        SubscribeLocalEvent<MothershipControlledComponent, MindAddedMessage>(OnControlledMindAdded);
        SubscribeLocalEvent<MothershipControlledComponent, MobStateChangedEvent>(OnControlledMobState);
        SubscribeLocalEvent<MothershipControlledComponent, EntityTerminatingEvent>(OnControlledTerminating);
    }

    private void OnTakeControl(Entity<MothershipCoreControlComponent> ent, ref RoboticsConsoleTakeControlMessage args)
    {
        if (ent.Comp.Controlled != null)
            return;

        if (!TryGetXenoborg(args.Address, out var target))
            return;

        if (!CanControl(target))
        {
            _popup.PopupEntity(Loc.GetString("mothership-core-control-busy"), ent, args.Actor);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        _mind.Visit(mindId, target);

        var controlled = EnsureComp<MothershipControlledComponent>(target);
        controlled.Core = ent;

        var action = controlled.ReturnActionEntity;
        _actions.AddAction(target, ref action, ent.Comp.ReturnAction);
        controlled.ReturnActionEntity = action;

        _toggle.TryActivate(target);
        SetGhostRolesTaken(target, true);

        ent.Comp.Controlled = target;
    }

    private void OnCoreMindRemoved(Entity<MothershipCoreControlComponent> ent, ref MindRemovedMessage args)
    {
        if (ent.Comp.Controlled is { } controlled && !TerminatingOrDeleted(controlled))
            Release(controlled);
    }

    private void OnReturn(Entity<MothershipControlledComponent> ent, ref ReturnToMothershipCoreEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Release(ent.Owner);
    }

    private void OnControlledMindAdded(Entity<MothershipControlledComponent> ent, ref MindAddedMessage args)
    {
        Release(ent.Owner);
    }

    private void OnControlledMobState(Entity<MothershipControlledComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            Release(ent.Owner);
    }

    private void OnControlledTerminating(Entity<MothershipControlledComponent> ent, ref EntityTerminatingEvent args)
    {
        ClearCore(ent);
    }

    /// <summary>
    /// Sends the core back into its own body and leaves the xenoborg empty again.
    /// </summary>
    public void Release(EntityUid uid)
    {
        if (!TryComp<MothershipControlledComponent>(uid, out var controlled))
            return;

        ClearCore((uid, controlled));

        if (controlled.Core is { } core
            && !TerminatingOrDeleted(core)
            && _mind.TryGetMind(core, out var mindId, out _))
            _mind.UnVisit(mindId);

        _actions.RemoveAction(uid, controlled.ReturnActionEntity);
        _toggle.TryDeactivate(uid);
        SetGhostRolesTaken(uid, false);

        RemCompDeferred<MothershipControlledComponent>(uid);
    }

    private void ClearCore(Entity<MothershipControlledComponent> ent)
    {
        if (ent.Comp.Core is not { } core || !TryComp<MothershipCoreControlComponent>(core, out var coreComp))
            return;

        if (coreComp.Controlled == ent.Owner)
            coreComp.Controlled = null;
    }

    private bool CanControl(EntityUid uid)
    {
        if (HasComp<MothershipCoreComponent>(uid) || HasComp<MothershipControlledComponent>(uid))
            return false;

        if (_mind.TryGetMind(uid, out _, out _) || HasComp<VisitingMindComponent>(uid))
            return false;

        return _mobState.IsAlive(uid);
    }

    private bool TryGetXenoborg(string address, out EntityUid borg)
    {
        var query = EntityQueryEnumerator<XenoborgComponent, DeviceNetworkComponent>();
        while (query.MoveNext(out var uid, out _, out var device))
        {
            if (device.Address != address)
                continue;

            borg = uid;
            return true;
        }

        borg = default;
        return false;
    }

    private void SetGhostRolesTaken(EntityUid uid, bool taken)
    {
        if (TryComp<GhostRoleComponent>(uid, out var role))
            _ghostRole.SetTaken(role, taken);

        if (!TryComp<BorgChassisComponent>(uid, out var chassis)
            || chassis.BrainEntity is not { } brain
            || !TryComp<GhostRoleComponent>(brain, out var brainRole))
            return;

        _ghostRole.SetTaken(brainRole, taken);
    }
}
