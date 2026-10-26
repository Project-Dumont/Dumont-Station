using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared.Weapons.Ranged.Events;
using System.Numerics;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Explosion;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphCrusherSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private ThrownItemSystem _thrown = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphCrusherComponent, XenomorphCrusherChargeActionEvent>(OnCharge);
        SubscribeLocalEvent<XenomorphCrusherComponent, XenomorphCrusherSoakActionEvent>(OnSoak);
        SubscribeLocalEvent<XenomorphCrusherComponent, XenomorphCrusherShieldActionEvent>(OnShield);
        SubscribeLocalEvent<XenomorphCrusherComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem) });
        SubscribeLocalEvent<XenomorphCrusherComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<XenomorphCrusherComponent, GetExplosionResistanceEvent>(OnExplosion);
        SubscribeLocalEvent<XenomorphCrusherComponent, ProjectileReflectAttemptEvent>(OnReflect);
        SubscribeLocalEvent<XenomorphCrusherComponent, HitScanReflectAttemptEvent>(OnReflectHitscan);
        SubscribeLocalEvent<XenomorphCrusherComponent, ThrowDoHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphCrusherComponent, StopThrowEvent>(OnStop);
        SubscribeLocalEvent<XenomorphCrusherComponent, MeleeHitEvent>(OnMelee);
    }

    private bool CanUse(Entity<XenomorphCrusherComponent> ent, int plasma)
        => _mobs.IsAlive(ent) && _blocker.CanAttack(ent) && _plasma.HasPlasma(ent, plasma)
           && (!TryComp<XenomorphRestComponent>(ent, out var rest) || !rest.Resting);

    private void OnCharge(Entity<XenomorphCrusherComponent> ent, ref XenomorphCrusherChargeActionEvent args)
    {
        if (args.Handled || ent.Comp.Charging || ent.Comp.ChargeTarget != null || !CanUse(ent, 20)
            || !_blocker.CanMove(ent) || Transform(ent).Anchored || !TryComp<PhysicsComponent>(ent, out _))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != target.MapId || (target.Position - origin.Position).LengthSquared() < 0.01f)
            return;
        _slow.TryUpdateMovementSpeedModDuration(ent, "XenomorphStompSlowdown", TimeSpan.FromSeconds(0.6), 0);
        ent.Comp.ChargeTarget = args.Target;
        ent.Comp.ChargeAt = _timing.CurTime + ent.Comp.ChargeDelay;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/crusher_windup_sound.ogg"), ent);
        args.Handled = true;
        SetActive<XenomorphCrusherChargeActionEvent>(ent, true);
    }

    private void OnSoak(Entity<XenomorphCrusherComponent> ent, ref XenomorphCrusherSoakActionEvent args)
    {
        if (args.Handled || !CanUse(ent, 20))
            return;
        ent.Comp.SoakUntil = _timing.CurTime + ent.Comp.SoakDuration;
        ent.Comp.SoakAccumulated = 0;
        SetActive<XenomorphCrusherSoakActionEvent>(ent, true);
        _flash.RaiseEffect(Color.FromHex("#421313"), new List<EntityUid> { ent }, Filter.Pvs(ent, entityManager: EntityManager), (float) ent.Comp.SoakDuration.TotalSeconds);
        args.Handled = true;
    }

    private void OnShield(Entity<XenomorphCrusherComponent> ent, ref XenomorphCrusherShieldActionEvent args)
    {
        if (args.Handled || !CanUse(ent, 50))
            return;
        ent.Comp.ShieldRemaining = ent.Comp.ShieldAmount;
        ent.Comp.ShieldUntil = _timing.CurTime + ent.Comp.ShieldDuration;
        ent.Comp.ExplosionUntil = _timing.CurTime + ent.Comp.ExplosionDuration;
        SetActive<XenomorphCrusherShieldActionEvent>(ent, true);
        _flash.RaiseEffect(Color.FromHex("#93653B"), new List<EntityUid> { ent }, Filter.Pvs(ent, entityManager: EntityManager), (float) ent.Comp.ShieldDuration.TotalSeconds);
        args.Handled = true;
    }

    private void OnDamage(Entity<XenomorphCrusherComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.ShieldRemaining <= 0 || ent.Comp.ShieldUntil <= _timing.CurTime)
            return;
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0)
                continue;
            var reduced = Math.Max(0, value.Float() - ent.Comp.ShieldReduction);
            var absorbed = Math.Min(reduced, ent.Comp.ShieldRemaining);
            ent.Comp.ShieldRemaining -= absorbed;
            args.Damage.DamageDict[type] = FixedPoint2.New(reduced - absorbed);
        }
        if (ent.Comp.ShieldRemaining <= 0)
            SetActive<XenomorphCrusherShieldActionEvent>(ent, false);
    }

    private void OnDamageChanged(Entity<XenomorphCrusherComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.SoakUntil <= _timing.CurTime || !args.DamageIncreased || args.DamageDelta == null || !_mobs.IsAlive(ent))
            return;
        ent.Comp.SoakAccumulated += args.DamageDelta.GetTotal().Float();
        if (ent.Comp.SoakAccumulated < ent.Comp.SoakDamageGoal)
            return;
        ent.Comp.SoakUntil = TimeSpan.Zero;
        SetActive<XenomorphCrusherSoakActionEvent>(ent, false);
        var remaining = ent.Comp.SoakHeal;
        var heal = new DamageSpecifier();
        foreach (var (type, amount) in args.Damageable.Damage.DamageDict)
        {
            if (amount <= 0 || remaining <= 0)
                continue;
            var restored = Math.Min(amount.Float(), remaining);
            heal.DamageDict[type] = FixedPoint2.New(-restored);
            remaining -= restored;
        }
        _damage.TryChangeDamage(ent, heal, ignoreResistances: true);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphTailStabActionEvent)
                _actions.ClearCooldown(action.AsNullable());
        _flash.RaiseEffect(Color.FromHex("#AD1313"), new List<EntityUid> { ent }, Filter.Pvs(ent, entityManager: EntityManager), 4);
    }

    private void OnExplosion(Entity<XenomorphCrusherComponent> ent, ref GetExplosionResistanceEvent args)
    {
        if (ent.Comp.ExplosionUntil > _timing.CurTime)
            args.DamageCoefficient /= (float) Math.Pow(1.1, 1000 / 5.0);
    }

    private void OnReflectHitscan(Entity<XenomorphCrusherComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected || ent.Comp.ShieldRemaining <= 0 || ent.Comp.ShieldUntil <= _timing.CurTime
            || HasComp<XenomorphComponent>(args.Shooter) || (-args.Direction).ToWorldAngle().GetCardinalDir()
                != _transform.GetWorldRotation(ent).GetCardinalDir() || !_random.Prob(.1f)) return;
        args.Direction = _random.NextAngle(-Angle.FromDegrees(90), Angle.FromDegrees(90)).Opposite().RotateVec(args.Direction);
        args.Reflected = true;
    }

    private void OnReflect(Entity<XenomorphCrusherComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || ent.Comp.ShieldRemaining <= 0 || ent.Comp.ShieldUntil <= _timing.CurTime
            || HasComp<XenomorphComponent>(args.Component.Shooter) || !TryComp<PhysicsComponent>(args.ProjUid, out var physics))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var projectile = _transform.GetMapCoordinates(args.ProjUid);
        if (origin.MapId != projectile.MapId || (projectile.Position - origin.Position).ToWorldAngle().GetCardinalDir()
            != _transform.GetWorldRotation(ent).GetCardinalDir() || !_random.Prob(0.1f))
            return;
        args.Cancelled = true;
        var velocity = _physics.GetMapLinearVelocity(args.ProjUid, component: physics);
        var reflected = _random.NextAngle(-Angle.FromDegrees(90), Angle.FromDegrees(90)).Opposite().RotateVec(velocity);
        _physics.SetLinearVelocity(args.ProjUid, reflected, body: physics);
        _transform.SetWorldRotation(args.ProjUid, reflected.ToWorldAngle() + args.Component.Angle);
        args.Component.Shooter = ent;
        args.Component.Weapon = ent;
        Dirty(args.ProjUid, args.Component);
    }

    private void OnHit(Entity<XenomorphCrusherComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!ent.Comp.Charging || args.Handled || HasComp<XenomorphComponent>(args.Target))
            return;
        args.Handled = true;
        var travelled = (_transform.GetMapCoordinates(ent).Position - ent.Comp.ChargeOrigin).Length();
        var direction = ent.Comp.ChargeDirection;
        var mob = HasComp<MobStateComponent>(args.Target);
        var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = FixedPoint2.New(ent.Comp.ChargeDamage) } };
        if (!mob)
            damage.DamageDict["Structural"] = FixedPoint2.New(ent.Comp.StructuralDamage);
        _damage.TryChangeDamage(args.Target, damage, origin: ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg"), ent);
        if (!mob && (TerminatingOrDeleted(args.Target) || EntityManager.IsQueuedForDeletion(args.Target)
            || !TryComp<PhysicsComponent>(args.Target, out var obstacle) || !obstacle.CanCollide))
        {
            if (TryComp<PhysicsComponent>(args.Target, out var destroyed) && EntityManager.IsQueuedForDeletion(args.Target))
                _physics.SetCanCollide(args.Target, false, body: destroyed);
            _physics.SetLinearVelocity(ent, direction * 30);
            return;
        }
        _thrown.StopThrow(ent, args.Component);
        if (!mob) return;
        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(2));
        if (!Transform(args.Target).Anchored)
            _throwing.TryThrow(args.Target, direction * (5 + Math.Clamp(travelled / ent.Comp.ChargeRange, 0, 1) * 5),
                15, user: ent, doSpin: false);
    }

    private void OnStop(Entity<XenomorphCrusherComponent> ent, ref StopThrowEvent args)
    {
        if (!ent.Comp.Charging)
            return;
        ent.Comp.Charging = false;
        SetActive<XenomorphCrusherChargeActionEvent>(ent, false);
        var nearby = new HashSet<Entity<MobStateComponent>>();
        _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(ent), 1.5f, nearby);
        foreach (var target in nearby)
        {
            if (HasComp<XenomorphComponent>(target) || _mobs.IsDead(target))
                continue;
            if (TryComp<MovementSpeedModifierComponent>(target, out var speed))
                _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(3.5),
                    1 / (1 + Math.Max(speed.CurrentSprintSpeed, speed.BaseSprintSpeed) * 0.4f));
        }
    }

    private void OnMelee(Entity<XenomorphCrusherComponent> ent, ref MeleeHitEvent args)
    {
        EntityUid? main = null;
        foreach (var target in args.HitEntities)
        {
            if (!HasComp<XenomorphComponent>(target) && HasComp<MobStateComponent>(target) && !_mobs.IsDead(target))
            {
                main = target;
                break;
            }
        }
        if (main is not { } primary)
            return;
        var nearby = new HashSet<Entity<MobStateComponent>>();
        _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(primary), 1.5f, nearby);
        var hits = 0;
        foreach (var target in nearby)
        {
            if (HasComp<XenomorphComponent>(target) || _mobs.IsDead(target) || args.HitEntities.Contains(target.Owner))
                continue;
            var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = 15 }, ArmorPenetration = 0.02f };
            if (_damage.TryChangeDamage(target, damage, origin: ent)?.GetTotal() > 0)
                _flash.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
            hits++;
        }
        foreach (var action in _actions.GetActions(ent))
        {
            var charge = _actions.GetEvent(action) is XenomorphCrusherChargeActionEvent;
            if ((!charge && _actions.GetEvent(action) is not XenomorphCrusherShieldActionEvent) || action.Comp.Cooldown is not { } cooldown)
                continue;
            var end = cooldown.End - TimeSpan.FromSeconds(1.5 + (charge ? hits * 0.5 : 0));
            if (end <= cooldown.Start)
                _actions.ClearCooldown(action.AsNullable());
            else
                _actions.SetCooldown(action.AsNullable(), cooldown.Start, end);
        }
    }

    private void SetActive<T>(EntityUid uid, bool active) where T : BaseActionEvent
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is T)
                _actions.SetToggled(action.Owner, active);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphCrusherComponent>();
        while (query.MoveNext(out var uid, out var crusher))
        {
            if (crusher.ChargeTarget is { } target && crusher.ChargeAt <= _timing.CurTime)
            {
                crusher.ChargeTarget = null;
                SetActive<XenomorphCrusherChargeActionEvent>(uid, false);
                if (_mobs.IsAlive(uid) && _blocker.CanMove(uid) && !Transform(uid).Anchored
                    && TryComp<PhysicsComponent>(uid, out var physics))
                {
                    var origin = _transform.GetMapCoordinates(uid);
                    var destination = _transform.ToMapCoordinates(target);
                    var direction = destination.Position - origin.Position;
                    if (origin.MapId == destination.MapId && direction.LengthSquared() > 0.01f)
                    {
                        var distance = Math.Min(direction.Length(), crusher.ChargeRange);
                        crusher.ChargeDirection = Vector2.Normalize(direction);
                        crusher.ChargeOrigin = origin.Position;
                        _physics.ResetDynamics(uid, physics);
                        crusher.Charging = true;
                        _throwing.TryThrow(uid, crusher.ChargeDirection * distance, 30, user: uid, pushbackRatio: 0, doSpin: false);
                        if (!HasComp<ThrownItemComponent>(uid))
                            crusher.Charging = false;
                        SetActive<XenomorphCrusherChargeActionEvent>(uid, crusher.Charging);
                    }
                }
            }
            if (crusher.SoakUntil != TimeSpan.Zero && crusher.SoakUntil <= _timing.CurTime)
            {
                crusher.SoakUntil = TimeSpan.Zero;
                SetActive<XenomorphCrusherSoakActionEvent>(uid, false);
            }
            if (crusher.ShieldRemaining > 0 && crusher.ShieldUntil <= _timing.CurTime)
            {
                crusher.ShieldRemaining = 0;
                SetActive<XenomorphCrusherShieldActionEvent>(uid, false);
            }
        }
    }
}
