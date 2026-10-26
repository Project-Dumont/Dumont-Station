using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server.DoAfter;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphBurrowSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphBurrowActionEvent>(OnBurrow);
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphBurrowDownDoAfterEvent>(OnBurrowed);
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphBurrowMoveDoAfterEvent>(OnMoved);
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphTremorActionEvent>(OnTremor);
        SubscribeLocalEvent<XenomorphBurrowComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphBurrowComponent, ActionAddedEvent>(OnActionAdded);
    }

    public bool ValidFloor(EntityCoordinates coordinates)
    {
        var tile = _turf.GetTileRef(coordinates);
        return tile is { } floor && !floor.Tile.IsEmpty && !_turf.IsTileBlocked(floor, CollisionGroup.Impassable);
    }

    private void OnBurrow(Entity<XenomorphBurrowComponent> ent, ref XenomorphBurrowActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)
            || _containers.IsEntityInContainer(ent) || ent.Comp.BuildingTunnel
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting)
            return;
        if (!ent.Comp.Active)
        {
            if (ent.Comp.NextBurrowAt > _timing.CurTime || Transform(ent).Anchored || !_blocker.CanMove(ent)
                || !ValidFloor(_transform.GetMoverCoordinates(ent)))
            {
                _popup.PopupEntity(Loc.GetString("xenomorph-burrow-invalid"), ent, ent);
                return;
            }
            args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.BurrowLength,
                new XenomorphBurrowDownDoAfterEvent(), ent.Owner) { BreakOnMove = true, NeedHand = false });
            return;
        }
        if (ent.Comp.Tunneling || ent.Comp.NextBurrowAt > _timing.CurTime || ent.Comp.NextTunnelAt > _timing.CurTime)
            return;
        var destination = _transform.ToMapCoordinates(args.Target);
        var from = _transform.GetMapCoordinates(ent);
        if (destination.MapId != from.MapId || _transform.GetGrid(args.Target) != Transform(ent).GridUid
            || !ValidFloor(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-burrow-destination"), ent, ent);
            return;
        }
        var distance = Vector2.Distance(from.Position, destination.Position);
        if (distance > ent.Comp.MaxTunnelingDistance)
            return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent,
            TimeSpan.FromSeconds(Math.Max(1, (int)distance)),
            new XenomorphBurrowMoveDoAfterEvent { Destination = GetNetCoordinates(args.Target) }, ent.Owner)
        { RequireCanInteract = false, NeedHand = false, DistanceThreshold = null });
        if (args.Handled)
        {
            ent.Comp.Tunneling = true;
            Dirty(ent);
        }
    }

    private void OnBurrowed(Entity<XenomorphBurrowComponent> ent, ref XenomorphBurrowDownDoAfterEvent args)
    {
        if (args.Handled)
            return;
        ent.Comp.NextBurrowAt = _timing.CurTime + ent.Comp.BurrowCooldown;
        if (args.Cancelled || !_mobs.IsAlive(ent) || _containers.IsEntityInContainer(ent)
            || !ValidFloor(_transform.GetMoverCoordinates(ent))
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting)
            return;
        ent.Comp.ForcedUnburrowAt = _timing.CurTime + ent.Comp.BurrowMaxDuration;
        SetActive(ent, true);
        args.Handled = true;
    }

    private void OnMoved(Entity<XenomorphBurrowComponent> ent, ref XenomorphBurrowMoveDoAfterEvent args)
    {
        if (args.Handled || !ent.Comp.Active)
            return;
        ent.Comp.Tunneling = false;
        if (args.Cancelled)
        {
            ent.Comp.NextTunnelAt = _timing.CurTime + ent.Comp.TunnelCooldown;
            Dirty(ent);
            return;
        }
        var destination = GetCoordinates(args.Destination);
        var from = _transform.GetMapCoordinates(ent);
        var to = _transform.ToMapCoordinates(destination);
        if (ValidFloor(destination) && _transform.GetGrid(destination) == Transform(ent).GridUid
            && to.MapId == from.MapId && Vector2.Distance(from.Position, to.Position) <= ent.Comp.MaxTunnelingDistance)
            _transform.SetCoordinates(ent, destination);
        else
            _popup.PopupEntity(Loc.GetString("xenomorph-burrow-destination"), ent, ent);
        SetActive(ent, false);
        args.Handled = true;
    }

    private void SetActive(Entity<XenomorphBurrowComponent> ent, bool active)
    {
        ent.Comp.Active = active;
        ent.Comp.Tunneling = false;
        _pulling.StopAllPulls(ent);
        if (TryComp<PhysicsComponent>(ent, out var physics))
            _physics.SetLinearVelocity(ent, Vector2.Zero, body: physics);
        if (!active)
        {
            ent.Comp.ForcedUnburrowAt = null;
            ent.Comp.NextBurrowAt = _timing.CurTime + ent.Comp.BurrowCooldown;
            if (_mobs.IsAlive(ent))
                foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(ent), ent.Comp.UnburrowStunRange))
                    if (!HasComp<XenomorphComponent>(target) && !_mobs.IsDead(target))
                        _stun.TryUpdateParalyzeDuration(target, ent.Comp.UnburrowStunLength);
        }
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
        _audio.PlayPvs(new SoundPathSpecifier(active ? "/Audio/_RMC14/Xeno/burrowing_b.ogg" : "/Audio/_RMC14/Xeno/burrowoff.ogg"), ent);
        if (!active)
        {
            foreach (var uid in ent.Comp.DisabledActions)
                if (Exists(uid)) _actions.SetEnabled(uid, true);
            ent.Comp.DisabledActions.Clear();
        }
        foreach (var action in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(action) is XenomorphBurrowActionEvent)
                _actions.SetToggled(action.Owner, active);
            else if (active && action.Comp.Enabled)
            {
                ent.Comp.DisabledActions.Add(action.Owner);
                _actions.SetEnabled(action.Owner, false);
            }
        }
    }

    private void OnActionAdded(Entity<XenomorphBurrowComponent> ent, ref ActionAddedEvent args)
    {
        if (!ent.Comp.Active || !args.Component.Enabled || _actions.GetEvent(args.Action) is XenomorphBurrowActionEvent)
            return;
        ent.Comp.DisabledActions.Add(args.Action);
        _actions.SetEnabled(args.Action, false);
    }

    private void OnTremor(Entity<XenomorphBurrowComponent> ent, ref XenomorphTremorActionEvent args)
    {
        if (args.Handled || ent.Comp.Active || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent)
            || !_plasma.ChangePlasmaAmount(ent, -100))
            return;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg"), ent);
        Spawn("DumontXenomorphTremorEffect", new EntityCoordinates(ent, Vector2.Zero));
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(ent), 4))
            if (!HasComp<XenomorphComponent>(target) && !_mobs.IsDead(target)
                && !HasComp<Content.Server._White.Xenomorphs.FaceHugger.FaceHuggerComponent>(target)
                && _interaction.InRangeUnobstructed(ent.Owner, target.Owner, 4,
                    predicate: uid => HasComp<XenomorphSprayAcidableComponent>(uid)))
                _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(2));
        args.Handled = true;
    }

    private void OnMobState(Entity<XenomorphBurrowComponent> ent, ref MobStateChangedEvent args)
    {
        if (ent.Comp.Active && args.NewMobState != MobState.Alive)
            SetActive(ent, false);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphBurrowComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (comp.Active && !comp.Tunneling && comp.ForcedUnburrowAt <= _timing.CurTime)
                SetActive((uid, comp), false);
    }
}
