// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ghost;
using Content.Shared._Dumont.Xenoborgs;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Xenoborgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Xenoborgs;

/// <summary>
/// Gives xenoborgs back to the ghosts when their player leaves.
/// </summary>
public sealed partial class XenoborgGhostRoleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private EntityQuery<ActorComponent> _actorQuery = default!;
    [Dependency] private EntityQuery<BorgChassisComponent> _chassisQuery = default!;
    [Dependency] private EntityQuery<GhostComponent> _ghostQuery = default!;
    [Dependency] private EntityQuery<MMIComponent> _mmiQuery = default!;

    [SubscribeLocalEvent]
    private void OnGhostAttempt(GhostAttemptHandleEvent args)
    {
        // no mob state, so it always counts as dead and would keep the core forever
        if (HasComp<MothershipCoreComponent>(args.Mind.OwnedEntity))
            args.CanReturnGlobal = false;
    }

    [SubscribeLocalEvent]
    private void OnMindAdded(Entity<XenoborgGhostRoleComponent> ent, ref MindAddedMessage args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnMindRemoved(Entity<XenoborgGhostRoleComponent> ent, ref MindRemovedMessage args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnPlayerAttached(Entity<XenoborgGhostRoleComponent> ent, ref PlayerAttachedEvent args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnPlayerDetached(Entity<XenoborgGhostRoleComponent> ent, ref PlayerDetachedEvent args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnInserted(Entity<XenoborgGhostRoleComponent> ent, ref EntInsertedIntoContainerMessage args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<XenoborgGhostRoleComponent> ent, ref EntRemovedFromContainerMessage args) => QueueCheck(ent);

    [SubscribeLocalEvent]
    private void OnMobStateChanged(Entity<XenoborgGhostRoleComponent> ent, ref MobStateChangedEvent args) => QueueCheck(ent);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenoborgGhostRoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextCheck is not { } next || next > now)
                continue;

            comp.NextCheck = null;
            UpdateRole((uid, comp), now);
        }
    }

    private void QueueCheck(Entity<XenoborgGhostRoleComponent> ent)
    {
        ent.Comp.NextCheck = _timing.CurTime;
    }

    private void UpdateRole(Entity<XenoborgGhostRoleComponent> ent, TimeSpan now)
    {
        if (!CanBeTaken(ent))
        {
            ent.Comp.AbandonedAt = null;
            SetOpen(ent, false);
            return;
        }

        if (!_mind.TryGetMind(ent, out _, out var mind))
        {
            ent.Comp.AbandonedAt = null;
            SetOpen(ent, true);
            return;
        }

        if (mind.VisitingEntity is not { } visiting || !_ghostQuery.HasComp(visiting))
        {
            ent.Comp.AbandonedAt = null;
            SetOpen(ent, false);
            return;
        }

        ent.Comp.AbandonedAt ??= now;
        var openAt = ent.Comp.AbandonedAt.Value + ent.Comp.ReturnGracePeriod;
        if (openAt > now)
        {
            ent.Comp.NextCheck = openAt;
            return;
        }

        SetOpen(ent, true);
    }

    private bool CanBeTaken(EntityUid uid)
    {
        if (_actorQuery.HasComp(uid) || HasComp<VisitingMindComponent>(uid) || !_mobState.IsAlive(uid))
            return false;

        return _chassisQuery.TryComp(uid, out var chassis) &&
            chassis.BrainEntity is { } brain &&
            _mmiQuery.TryComp(brain, out var mmi) &&
            mmi.BrainSlot.HasItem;
    }

    private void SetOpen(Entity<XenoborgGhostRoleComponent> ent, bool open)
    {
        if (ent.Comp.Open == open)
            return;

        ent.Comp.Open = open;
        if (open)
            EntityManager.AddComponents(ent, ent.Comp.Components);
        else
            EntityManager.RemoveComponents(ent, ent.Comp.Components);
    }
}
