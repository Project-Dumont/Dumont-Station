using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._White.Xenomorphs.Plasma;
using Robust.Shared.Audio.Systems;
using Content.Shared._White.Xenomorphs.Stealth;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using Content.Shared.Physics;
using System.Linq;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphLeapSystem : EntitySystem
{
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPlasmaSystem _plasma = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private ThrownItemSystem _thrownItem = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphLeapComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<XenomorphLeapComponent, XenomorphLeapActionEvent>(OnLeap);
        SubscribeLocalEvent<XenomorphLeapComponent, ThrowDoHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphLeapComponent, StopThrowEvent>(OnStop);
        SubscribeLocalEvent<XenomorphLeapComponent, UpdateCanMoveEvent>(OnCanMove);
        SubscribeLocalEvent<XenomorphLeapComponent, MeleeHitEvent>(OnMelee);
    }

    private void OnLeap(Entity<XenomorphLeapComponent> ent, ref XenomorphLeapActionEvent args)
    {
        if (_net.IsClient || args.Handled || ent.Comp.Leaping || !_mobState.IsAlive(ent)
            || !_blocker.CanMove(ent) || Transform(ent).Anchored
            || _container.IsEntityInContainer(ent) || !HasComp<PhysicsComponent>(ent))
            return;

        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        var length = direction.Length();
        if (origin.MapId != target.MapId || !float.IsFinite(length) || length <= 0
            || ent.Comp.Range <= 0 || ent.Comp.Speed <= 0
            || ent.Comp.PlasmaCost > 0 && !_plasma.HasPlasma(ent, ent.Comp.PlasmaCost))
            return;

        direction *= Math.Min(length, ent.Comp.Range) / length;
        _throwing.TryThrow(ent, direction, ent.Comp.Speed, ent, pushbackRatio: 0f, doSpin: false);
        if (!HasComp<ThrownItemComponent>(ent))
            return;

        if (ent.Comp.PlasmaCost > 0) _plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaCost);
        if (ent.Comp.Sound != null) _audio.PlayPvs(ent.Comp.Sound, ent);
        ent.Comp.Leaping = true;
        _blocker.UpdateCanMove(ent);
        Dirty(ent);
        args.Handled = true;
        var started = new XenomorphLeapStartedEvent();
        RaiseLocalEvent(ent, ref started);
    }

    private void OnHit(Entity<XenomorphLeapComponent> ent, ref ThrowDoHitEvent args)
    {
        if (_net.IsClient || args.Handled || !ent.Comp.Leaping)
            return;

        args.Handled = true;
        if (HasComp<XenomorphComponent>(args.Target))
            return;

        _thrownItem.StopThrow(ent, args.Component);
        if (Transform(args.Target).Anchored || !HasComp<MobStateComponent>(args.Target))
        {
            if (ent.Comp.ObstacleStun > TimeSpan.Zero) _stun.TryUpdateParalyzeDuration(ent, ent.Comp.ObstacleStun);
            return;
        }
        if (ent.Comp.KnockdownRequiresStealth
            && (!TryComp<XenomorphInvisibilityComponent>(ent, out var stealth) || !stealth.Active))
            return;

        if (ent.Comp.KnockdownTime > TimeSpan.Zero) _stun.TryKnockdown(args.Target, ent.Comp.KnockdownTime, true);
        ent.Comp.RootedUntil = _timing.CurTime + ent.Comp.MoveDelay;
        var ev = new XenomorphLeapHitEvent(args.Target);
        RaiseLocalEvent(ent, ref ev);
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
    }

    private void OnStop(Entity<XenomorphLeapComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.Leaping = false;
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
    }

    private void OnMove(Entity<XenomorphLeapComponent> ent, ref MoveEvent args)
    {
        if (_net.IsClient || !ent.Comp.Leaping || ent.Comp.CheckingMove) return;
        var old = _transform.ToMapCoordinates(args.OldPosition);
        var next = _transform.ToMapCoordinates(args.NewPosition);
        var delta = next.Position - old.Position;
        var length = delta.Length();
        if (old.MapId != next.MapId || length < .001f) return;
        var direction = delta / length;
        var side = new Vector2(-direction.Y, direction.X) * .22f;
        var nearest = float.MaxValue;
        foreach (var offset in new[] { Vector2.Zero, side, -side })
        {
            var ray = new CollisionRay(old.Position + offset, direction, (int)CollisionGroup.Impassable);
            foreach (var hit in _physics.IntersectRayWithPredicate(old.MapId, ray, length + .22f,
                uid => uid == ent.Owner || HasComp<MobStateComponent>(uid)
                    || !TryComp<FixturesComponent>(uid, out var fixtures) || !fixtures.Fixtures.Values.Any(f => f.Hard), false))
                nearest = Math.Min(nearest, hit.Distance);
        }
        if (nearest == float.MaxValue) return;
        ent.Comp.CheckingMove = true;
        _transform.SetCoordinates(ent, _transform.ToCoordinates(args.OldPosition.EntityId,
            new Robust.Shared.Map.MapCoordinates(old.Position + direction * Math.Max(0, nearest - .25f), old.MapId)));
        ent.Comp.CheckingMove = false;
        if (TryComp<ThrownItemComponent>(ent, out var thrown)) _thrownItem.StopThrow(ent, thrown);
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        if (ent.Comp.ObstacleStun > TimeSpan.Zero) _stun.TryUpdateParalyzeDuration(ent, ent.Comp.ObstacleStun);
    }

    private void OnCanMove(Entity<XenomorphLeapComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Leaping || ent.Comp.RootedUntil > _timing.CurTime)
            args.Cancel();
    }

    private void OnMelee(Entity<XenomorphLeapComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.UnrootOnMelee && args.User == ent.Owner && args.IsHit)
            Unroot(ent);
    }

    private void Unroot(Entity<XenomorphLeapComponent> ent)
    {
        ent.Comp.RootedUntil = TimeSpan.Zero;
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphLeapComponent>();
        while (query.MoveNext(out var uid, out var leap))
        {
            if (leap.RootedUntil != TimeSpan.Zero && leap.RootedUntil <= _timing.CurTime)
                Unroot((uid, leap));
        }
    }
}
