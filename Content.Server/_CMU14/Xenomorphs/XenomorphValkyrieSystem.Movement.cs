using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphRetrievedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Until;
}

public sealed partial class XenomorphValkyrieSystem
{
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private TagSystem _tags = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    private readonly HashSet<EntityUid> _hits = new();

    private void InitializeMovement()
    {
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphHighGallopActionEvent>(OnGallop);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphRetrieveActionEvent>(OnRetrieve);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphRetrieveDoAfterEvent>(OnRetrieved);
    }
    private void OnGallop(Entity<XenomorphValkyrieComponent> ent, ref XenomorphHighGallopActionEvent args)
    {
        if (args.Handled || !Ready(ent) || Transform(ent).GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != target.MapId) return;
        var from = _transform.ToCoordinates(gridUid, origin);
        var to = _transform.ToCoordinates(gridUid, target);
        var delta = to.Position - from.Position;
        if (!float.IsFinite(delta.LengthSquared()) || delta.LengthSquared() < 0.0001f) return;
        var area = Box2.CenteredAround(from.Position, new Vector2(3, 2)).Translated(new Vector2(0, 1.5f));
        var rotated = new Box2Rotated(area, Vector2.Normalize(delta).ToAngle() - Angle.FromDegrees(90), from.Position);
        var bounds = rotated.CalcBoundingBox();
        foreach (var tile in _map.GetTilesIntersecting(gridUid, grid, rotated))
        {
            var point = _turf.GetTileCenter(tile);
            Spawn(bounds.Encloses(Box2.CenteredAround(point.Position, Vector2.One)) ? "DumontXenomorphGallopTelegraph" : "DumontXenomorphGallopEdgeTelegraph", point);
        }
        _hits.Clear();
        _lookup.GetEntitiesIntersecting(gridUid, rotated, _hits);
        foreach (var uid in _hits)
        {
            if (!_interaction.InRangeUnobstructed(ent.Owner, uid, 6, collisionMask: CollisionGroup.Impassable)) continue;
            if (_tags.HasTag(uid, "Grenade"))
            {
                _pulling.StopAllPulls(uid);
                var direction = _transform.GetWorldPosition(uid) - origin.Position;
                if (direction.LengthSquared() > 0 && !Transform(uid).Anchored) _throwing.TryThrow(uid, Vector2.Normalize(direction) * 3, 10, ent, pushbackRatio: 0, doSpin: false);
            }
            else if (HasComp<MobStateComponent>(uid) && !HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid))
            {
                _stun.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(1));
                var speed = TryComp<MovementSpeedModifierComponent>(uid, out var movement) ? movement.BaseSprintSpeed : 3.5f;
                _slow.TryUpdateMovementSpeedModDuration(uid, "XenomorphValkyrieSlowdown", TimeSpan.FromSeconds(2.5), 1 / (1 + speed * 0.5f));
            }
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge3.ogg"), ent);
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
        args.Handled = true;
    }
    private bool CanRetrieve(EntityUid user, EntityUid target, float range)
    {
        if (user == target || !Ally(target) || Transform(target).Anchored || HasComp<XenomorphRetrievedComponent>(target)) return false;
        if (Comp<XenomorphComponent>(target).Caste == "Queen" && _mobs.IsAlive(target) && !_standing.IsDown(target)
            && !(TryComp<XenomorphRestComponent>(target, out var rest) && rest.Resting)) return false;
        return _interaction.InRangeUnobstructed(user, target, range, collisionMask: CollisionGroup.Impassable);
    }
    private void OnRetrieve(Entity<XenomorphValkyrieComponent> ent, ref XenomorphRetrieveActionEvent args)
    {
        if (args.Handled || ent.Comp.RetrieveAction != null || !Ready(ent) || !CanRetrieve(ent, args.Target, 7)) return;
        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(0.6), new XenomorphRetrieveDoAfterEvent(), ent, args.Target)
            { NeedHand = false, BreakOnMove = true, DistanceThreshold = 10 })) return;
        ent.Comp.RetrieveAction = args.Action.Owner;
        _actions.SetToggled(args.Action.Owner, true);
        var origin = _transform.GetMapCoordinates(ent);
        var delta = _transform.GetMapCoordinates(args.Target).Position - origin.Position;
        if (Transform(ent).GridUid is { } gridUid && delta.LengthSquared() > 0)
        {
            for (var i = 1; i <= MathF.Ceiling(delta.Length()); i++)
            {
                var world = origin.Position + Vector2.Normalize(delta) * i;
                var local = _transform.ToCoordinates(gridUid, new MapCoordinates(world, origin.MapId));
                var point = new Vector2(MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
                ent.Comp.Telegraphs.Add(Spawn("DumontXenomorphRetrieveTelegraph", new EntityCoordinates(gridUid, point)));
            }
        }
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
        args.Handled = true;
    }
    private void ClearRetrieve(Entity<XenomorphValkyrieComponent> ent)
    {
        foreach (var uid in ent.Comp.Telegraphs) if (Exists(uid)) QueueDel(uid);
        ent.Comp.Telegraphs.Clear();
        if (ent.Comp.RetrieveAction is { } action && Exists(action)) _actions.SetToggled(action, false);
        ent.Comp.RetrieveAction = null;
    }
    private void OnRetrieved(Entity<XenomorphValkyrieComponent> ent, ref XenomorphRetrieveDoAfterEvent args)
    {
        if (args.Handled) return;
        var action = ent.Comp.RetrieveAction;
        ClearRetrieve(ent);
        if (args.Cancelled || args.Target is not { } target || action == null || !Ready(ent) || !CanRetrieve(ent, target, 10)
            || !TryComp<PhysicsComponent>(target, out var physics) || !Spend(ent, 100, 180)) return;
        var delta = _transform.GetWorldPosition(ent) - _transform.GetWorldPosition(target);
        if (delta.LengthSquared() <= 0) return;
        _pulling.StopAllPulls(target);
        var distance = Math.Clamp(delta.Length() + 1, 0.1f, 10);
        var retrieved = EnsureComp<XenomorphRetrievedComponent>(target);
        retrieved.Until = _timing.CurTime + TimeSpan.FromSeconds(distance / 15);
        _physics.ApplyLinearImpulse(target, Vector2.Normalize(delta) * 15 * physics.Mass, body: physics);
        _physics.SetBodyStatus(target, physics, BodyStatus.InAir);
        _actions.SetCooldown(action.Value, TimeSpan.FromSeconds(10));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg"), ent);
        args.Handled = true;
    }
    private void UpdateRetrieve()
    {
        var query = EntityQueryEnumerator<XenomorphRetrievedComponent>();
        while (query.MoveNext(out var uid, out var retrieved))
        {
            if (_timing.CurTime < retrieved.Until) continue;
            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
                _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround);
            }
            RemCompDeferred<XenomorphRetrievedComponent>(uid);
        }
    }
}
