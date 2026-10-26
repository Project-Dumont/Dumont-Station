using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server.Destructible;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Buckle.Components;
using Content.Shared.Maps;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphRavagerSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private DestructibleSystem _destructible = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRavagerComponent, XenomorphEmpowerActionEvent>(OnEmpower);
        SubscribeLocalEvent<XenomorphRavagerComponent, XenomorphScissorCutActionEvent>(OnScissor);
        SubscribeLocalEvent<XenomorphRavagerComponent, XenomorphLeapHitEvent>(OnLeapHit);
        SubscribeLocalEvent<XenomorphRavagerComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphRavagerComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem), typeof(XenomorphReaperSystem) });
        SubscribeLocalEvent<XenomorphRavagerComponent, ThrowDoHitEvent>(OnObstacle, before: new[] { typeof(XenomorphLeapSystem) });
        SubscribeLocalEvent<XenomorphRavagerComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<XenomorphRavagerComponent, IgnitedEvent>(OnIgnited);
    }

    private void OnIgnited(Entity<XenomorphRavagerComponent> ent, ref IgnitedEvent args)
    {
        if (!TryComp<PlasmaVesselComponent>(ent, out var plasma) || plasma.Plasma >= plasma.MaxPlasma) return;
        _plasma.ChangePlasmaAmount(ent, plasma.MaxPlasma - plasma.Plasma);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_roar1.ogg"), ent);
    }

    private bool CanUse(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid)
        && !(TryComp<XenomorphRestComponent>(uid, out var rest) && rest.Resting);

    private bool Enemy(EntityUid uid) => HasComp<MobStateComponent>(uid) && !HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid)
        && !_containers.IsEntityInContainer(uid)
        && !(TryComp<BuckleComponent>(uid, out var buckle) && buckle.BuckledTo is { } nest && HasComp<XenomorphNestComponent>(nest));

    private void ToggleEmpower(EntityUid uid, bool active)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphEmpowerActionEvent)
                _actions.SetToggled(action.AsNullable(), active);
    }

    private void ToggleEmpoweredActions(EntityUid uid, bool active)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphLeapActionEvent or XenomorphScissorCutActionEvent)
                _actions.SetToggled(action.AsNullable(), active);
    }

    private void OnEmpower(Entity<XenomorphRavagerComponent> ent, ref XenomorphEmpowerActionEvent args)
    {
        if (args.Handled || !CanUse(ent)) return;
        if (ent.Comp.EmpowerAt != null)
        {
            CompleteEmpower(ent);
            args.Handled = true;
            return;
        }
        if (!_plasma.ChangePlasmaAmount(ent, -50)) return;
        _actions.SetUseDelay(args.Action.AsNullable(), null);
        ent.Comp.FirstActivation = _timing.CurTime;
        ent.Comp.EmpowerAt = _timing.CurTime + TimeSpan.FromSeconds(6);
        ApplyShield(ent, 50);
        ToggleEmpower(ent, true);
        args.Handled = true;
    }

    private void ApplyShield(Entity<XenomorphRavagerComponent> ent, int amount)
    {
        ent.Comp.Shield = Math.Max(ent.Comp.Shield, amount);
        ent.Comp.ShieldUntil = _timing.CurTime + TimeSpan.FromSeconds(15);
        if (ent.Comp.Shield > 0 && ent.Comp.ShieldEffect == null)
            ent.Comp.ShieldEffect = Spawn("DumontXenomorphRavagerShield", new EntityCoordinates(ent, Vector2.Zero));
    }

    private void CompleteEmpower(Entity<XenomorphRavagerComponent> ent)
    {
        ent.Comp.EmpowerAt = null;
        ToggleEmpower(ent, false);
        var targets = _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(ent), 5.078f)
            .Where(t => Enemy(t) && _interaction.InRangeUnobstructed(ent.Owner, t.Owner, 5.078f))
            .OrderBy(t => (_transform.GetMapCoordinates(t).Position - _transform.GetMapCoordinates(ent).Position).LengthSquared()).Take(6).ToArray();
        ApplyShield(ent, targets.Length * 50);
        Spawn("DumontXenomorphEmpowerEffect", new EntityCoordinates(ent, Vector2.Zero));
        foreach (var target in targets)
            Spawn("DumontXenomorphEmpowerTelegraph", _transform.GetMoverCoordinates(target));
        if (targets.Length >= 3)
        {
            ToggleEmpoweredActions(ent, true);
            _popup.PopupEntity(Loc.GetString("xenomorph-ravager-super-empower"), ent, ent);
            ent.Comp.EmpoweredTargets = targets.Length;
            ent.Comp.SuperUntil = _timing.CurTime + TimeSpan.FromSeconds(6.5);
            if (ent.Comp.SuperEffect is { } old) QueueDel(old);
            ent.Comp.SuperEffect = Spawn("DumontXenomorphSuperEmpowerEffect", new EntityCoordinates(ent, Vector2.Zero));
            _flash.RaiseEffect(Color.FromHex("#FF0000"), new List<EntityUid> { ent }, Filter.Pvs(ent, entityManager: EntityManager), 5);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_roar1.ogg"), ent);
            Dirty(ent);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-ravager-empower-targets", ("targets", targets.Length)), ent, ent);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_swipe1.ogg"), ent);
        }
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphEmpowerActionEvent)
            {
                _actions.SetUseDelay(action.AsNullable(), ent.Comp.FirstActivation + TimeSpan.FromSeconds(18) - _timing.CurTime);
                _actions.SetCooldown(action.AsNullable(), ent.Comp.FirstActivation, ent.Comp.FirstActivation + TimeSpan.FromSeconds(18));
            }
    }

    private void OnDamage(Entity<XenomorphRavagerComponent> ent, ref DamageModifyEvent args)
    {
        if (args.Damage.GetTotal() <= 0) return;
        ent.Comp.ShieldUntil = _timing.CurTime + TimeSpan.FromSeconds(15);
        if (ent.Comp.Shield <= 0) return;
        args.Damage = new DamageSpecifier(args.Damage);
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0) continue;
            var absorbed = Math.Min(value.Float(), ent.Comp.Shield);
            ent.Comp.Shield -= absorbed;
            args.Damage.DamageDict[type] -= FixedPoint2.New(absorbed);
        }
        if (ent.Comp.Shield <= 0) ClearShield(ent);
    }

    private void ClearShield(Entity<XenomorphRavagerComponent> ent)
    {
        ent.Comp.Shield = 0;
        if (ent.Comp.ShieldEffect is { } effect) QueueDel(effect);
        ent.Comp.ShieldEffect = null;
    }

    private void OnMelee(Entity<XenomorphRavagerComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner || !args.HitEntities.Any(Enemy)) return;
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphLeapActionEvent && action.Comp.Cooldown is { } cooldown)
            {
                var end = cooldown.End - TimeSpan.FromSeconds(3);
                if (end <= _timing.CurTime) _actions.ClearCooldown(action.AsNullable());
                else _actions.SetCooldown(action.AsNullable(), cooldown.Start, end);
            }
    }

    private void OnLeapHit(Entity<XenomorphRavagerComponent> ent, ref XenomorphLeapHitEvent args)
    {
        if (!Enemy(args.Target) || _timing.CurTime >= ent.Comp.SuperUntil) return;
        _damage.TryChangeDamage(args.Target, new DamageSpecifier { DamageDict = { ["Blunt"] = 15, ["Slash"] = 15, ["Piercing"] = 15 } }, origin: ent);
        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(3.2));
        var direction = _transform.GetMapCoordinates(args.Target).Position - _transform.GetMapCoordinates(ent).Position;
        if (direction.LengthSquared() > 0.0001f)
            _throwing.TryThrow(args.Target, Vector2.Normalize(direction) * 3, 10, ent, doSpin: false);
        Spawn("DumontXenomorphRaptureEffect", new EntityCoordinates(args.Target, Vector2.Zero));
    }

    private bool DestroyObstacle(EntityUid uid, EntityUid attacker)
    {
        var threshold = _destructible.DestroyedAt(uid);
        if (threshold != FixedPoint2.MaxValue && HasComp<DamageableComponent>(uid))
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Blunt"] = threshold } }, ignoreResistances: true, origin: attacker);
        return EntityManager.IsQueuedForDeletion(uid) || _destructible.DestroyEntity(uid);
    }

    private void OnObstacle(Entity<XenomorphRavagerComponent> ent, ref ThrowDoHitEvent args)
    {
        if (args.Handled || !TryComp<XenomorphLeapComponent>(ent, out var leap) || !leap.Leaping
            || !TryComp<XenomorphRavagerObstacleComponent>(args.Target, out var obstacle) || !obstacle.Assault) return;
        if (!DestroyObstacle(args.Target, ent)) return;
        _physics.SetCanCollide(args.Target, false);
        args.Handled = true;
    }

    private void OnScissor(Entity<XenomorphRavagerComponent> ent, ref XenomorphScissorCutActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || Transform(ent).GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || direction.LengthSquared() < 0.0001f || !float.IsFinite(direction.LengthSquared())
            || !_plasma.ChangePlasmaAmount(ent, -25)) return;
        direction = Vector2.Normalize(direction);
        var side = new Vector2(-direction.Y, direction.X);
        var candidates = _lookup.GetEntitiesInRange(_transform.GetMoverCoordinates(ent), 4.6f)
            .Where(t => t != ent.Owner)
            .Select(t => (Uid: t, Offset: _transform.GetMapCoordinates(t).Position - origin.Position))
            .Where(t => Vector2.Dot(t.Offset, direction) >= 0.5f && Vector2.Dot(t.Offset, direction) <= 4.5f && Math.Abs(Vector2.Dot(t.Offset, side)) <= 0.5f)
            .OrderBy(t => t.Offset.LengthSquared()).ToArray();
        foreach (var (uid, _) in candidates)
        {
            if (!_interaction.InRangeUnobstructed(ent.Owner, uid, 4.6f)) continue;
            if (TryComp<XenomorphRavagerObstacleComponent>(uid, out var obstacle) && obstacle.Scissor)
            {
                if (obstacle.ScissorDamage is { } damage)
                    _damage.TryChangeDamage(uid, damage, origin: ent);
                else if (DestroyObstacle(uid, ent))
                    _physics.SetCanCollide(uid, false);
                continue;
            }
            if (!Enemy(uid)) continue;
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Slash"] = 75 } }, origin: ent);
            Spawn("DumontXenomorphRaptureEffect", new EntityCoordinates(uid, Vector2.Zero));
            _flash.RaiseEffect(Color.Red, new List<EntityUid> { uid }, Filter.Pvs(uid, entityManager: EntityManager));
            if (_timing.CurTime < ent.Comp.SuperUntil)
            {
                var speed = TryComp<MovementSpeedModifierComponent>(uid, out var movement)
                    ? Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) : 3.5f;
                _slow.TryUpdateMovementSpeedModDuration(uid, "XenomorphStompSlowdown", TimeSpan.FromSeconds(5), 1 / (1 + speed * 0.4f));
            }
        }
        var area = Box2.CenteredAround(origin.Position, new Vector2(1, 4)).Translated(new Vector2(0, 2.5f));
        var rotated = new Box2Rotated(area, direction.ToAngle() - Angle.FromDegrees(90), origin.Position);
        foreach (var tile in _map.GetTilesIntersecting(gridUid, grid, rotated))
        {
            var point = _turf.GetTileCenter(tile);
            if (_interaction.InRangeUnobstructed(origin, _transform.ToMapCoordinates(point), 4.6f))
                Spawn("DumontXenomorphScissorTelegraph", point);
        }
        _audio.PlayPvs(new SoundCollectionSpecifier("AlienClaw"), ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_roar1.ogg"), ent);
        var xform = Transform(ent);
        _melee.DoLunge(ent, ent, ent, Angle.Zero, (-_transform.GetWorldRotation(xform)).RotateVec(direction * 4), "WeaponArcThrust", Angle.Zero, false, predicted: false);
        args.Handled = true;
    }

    private void OnRemove(Entity<XenomorphRavagerComponent> ent, ref ComponentRemove args)
    {
        ClearShield(ent);
        ToggleEmpoweredActions(ent, false);
        if (ent.Comp.SuperEffect is { } effect) QueueDel(effect);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphRavagerComponent>();
        while (query.MoveNext(out var uid, out var ravager))
        {
            if (ravager.EmpowerAt is { } at && _timing.CurTime >= at)
            {
                if (_mobs.IsAlive(uid)) CompleteEmpower((uid, ravager));
                else { ravager.EmpowerAt = null; ToggleEmpower(uid, false); }
            }
            if (ravager.Shield > 0 && _timing.CurTime >= ravager.ShieldUntil) ClearShield((uid, ravager));
            if (ravager.EmpoweredTargets > 0 && _timing.CurTime >= ravager.SuperUntil)
            {
                ToggleEmpoweredActions(uid, false);
                ravager.EmpoweredTargets = 0;
                ravager.SuperEffect = null;
                Dirty(uid, ravager);
            }
        }
    }
}
