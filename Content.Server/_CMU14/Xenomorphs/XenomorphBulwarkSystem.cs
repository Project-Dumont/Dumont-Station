using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared.Weapons.Ranged.Events;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphBulwarkSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private TagSystem _tags = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBulwarkComponent, XenomorphEncasedPlatesActionEvent>(OnPlates);
        SubscribeLocalEvent<XenomorphBulwarkComponent, XenomorphPlateBashActionEvent>(OnBash);
        SubscribeLocalEvent<XenomorphBulwarkComponent, XenomorphBulwarkTailSwingActionEvent>(OnTail);
        SubscribeLocalEvent<XenomorphBulwarkComponent, XenomorphReflectiveShieldActionEvent>(OnShield);
        SubscribeLocalEvent<XenomorphBulwarkComponent, ProjectileReflectAttemptEvent>(OnReflect);
        SubscribeLocalEvent<XenomorphBulwarkComponent, HitScanReflectAttemptEvent>(OnReflectHitscan);
        SubscribeLocalEvent<XenomorphBulwarkComponent, MobStateChangedEvent>(OnMobState);
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid)
        && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting);

    private bool IsPrey(EntityUid uid) => HasComp<MobStateComponent>(uid)
        && !HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid);

    private void OnPlates(Entity<XenomorphBulwarkComponent> ent, ref XenomorphEncasedPlatesActionEvent args)
    {
        if (args.Handled || !Ready(ent))
            return;
        SetEncased(ent, !ent.Comp.Encased);
        args.Handled = true;
    }

    private void SetEncased(Entity<XenomorphBulwarkComponent> ent, bool encased)
    {
        ent.Comp.Encased = encased;
        if (!encased && ent.Comp.Reflecting)
            StopReflecting(ent, true);
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        _armor.RefreshAlert(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphEncasedPlatesActionEvent)
                _actions.SetToggled(action.Owner, encased);
    }

    private void OnBash(Entity<XenomorphBulwarkComponent> ent, ref XenomorphPlateBashActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !IsPrey(args.Target))
            return;
        var range = ent.Comp.Encased ? ent.Comp.PlateBashEncasedRange : ent.Comp.PlateBashUnencasedLeapRange;
        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-bulwark-bash-range"), ent, ent);
            return;
        }
        _audio.PlayPvs(ent.Comp.PlateBashSound, ent);
        if (!ent.Comp.Encased)
            ThrowAway(ent, args.Target, ent.Comp.PlateBashUnencasedLeapRange, ent.Comp.PlateBashUnencasedLeapSpeed, ent);
        var damage = new DamageSpecifier(ent.Comp.PlateBashDamage) { ArmorPenetration = 0.1f };
        _damage.TryChangeDamage(args.Target, damage, origin: ent);
        _stun.TryUpdateParalyzeDuration(args.Target, ent.Comp.PlateBashParalyzeTime);
        ThrowAway(args.Target, ent, ent.Comp.Encased ? ent.Comp.PlateBashEncasedKnockBackDistance
            : ent.Comp.PlateBashUnencasedKnockBackDistance, ent.Comp.PlateBashKnockBackSpeed, ent);
        args.Handled = true;
    }

    private void ThrowAway(EntityUid target, EntityUid origin, float range, float speed, EntityUid user)
    {
        var direction = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(origin);
        if (target == user)
        {
            range = Math.Min(range, direction.Length());
            direction = -direction;
        }
        if (!Transform(target).Anchored && direction.LengthSquared() > 0)
            _throwing.TryThrow(target, Vector2.Normalize(direction) * range, speed, user, pushbackRatio: 0, doSpin: false);
    }

    private void OnTail(Entity<XenomorphBulwarkComponent> ent, ref XenomorphBulwarkTailSwingActionEvent args)
    {
        if (args.Handled || !Ready(ent))
            return;
        if (ent.Comp.Encased)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-bulwark-tail-encased"), ent, ent);
            return;
        }
        var hit = false;
        EnsureComp<XenomorphSweepingComponent>(ent);
        _audio.PlayPvs(ent.Comp.TailSwingSound, ent);
        foreach (var target in _lookup.GetEntitiesInRange(_transform.GetMoverCoordinates(ent), ent.Comp.TailSwingRange))
        {
            if (target == ent.Owner || !_interaction.InRangeUnobstructed(ent.Owner, target, ent.Comp.TailSwingRange))
                continue;
            if (_tags.HasTag(target, ent.Comp.TailSwingFlingable))
            {
                ThrowAway(target, ent, ent.Comp.TailSwingFlingDistance, ent.Comp.TailSwingFlingSpeed, ent);
                hit = true;
            }
            else if (IsPrey(target))
            {
                _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Slash"] = 20 } }, origin: ent);
                _stun.TryUpdateParalyzeDuration(target, ent.Comp.TailSwingParalyzeTime);
                hit = true;
            }
        }
        _actions.SetUseDelay(args.Action.Owner, hit ? TimeSpan.FromSeconds(12) : ent.Comp.TailSwingMissCooldown);
        args.Handled = true;
    }

    private void OnShield(Entity<XenomorphBulwarkComponent> ent, ref XenomorphReflectiveShieldActionEvent args)
    {
        if (args.Handled || !Ready(ent))
            return;
        if (!ent.Comp.Encased)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-bulwark-shield-encase"), ent, ent);
            return;
        }
        args.Handled = true;
        if (ent.Comp.Reflecting)
        {
            StopReflecting(ent, true);
            return;
        }
        ent.Comp.Reflecting = true;
        ent.Comp.ReflectStartedAt = _timing.CurTime;
        ent.Comp.ReflectExpiresAt = _timing.CurTime + ent.Comp.ReflectDuration;
        ent.Comp.ReflectEffect = Spawn(ent.Comp.ReflectEffectId, new EntityCoordinates(ent, Vector2.Zero));
        Dirty(ent);
        _actions.SetUseDelay(args.Action.Owner, TimeSpan.FromSeconds(1));
        _actions.SetToggled(args.Action.Owner, true);
    }

    private void StopReflecting(Entity<XenomorphBulwarkComponent> ent, bool refund)
    {
        ent.Comp.Reflecting = false;
        if (ent.Comp.ReflectEffect is { } effect)
            QueueDel(effect);
        ent.Comp.ReflectEffect = null;
        Dirty(ent);
        var cooldown = refund
            ? TimeSpan.FromSeconds(Math.Max(ent.Comp.ReflectMinCooldown.TotalSeconds,
                (_timing.CurTime - ent.Comp.ReflectStartedAt).TotalSeconds * ent.Comp.ReflectCooldownPerSecond))
            : ent.Comp.ReflectFullCooldown;
        foreach (var action in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(action) is not XenomorphReflectiveShieldActionEvent)
                continue;
            _actions.SetToggled(action.Owner, false);
            _actions.SetUseDelay(action.Owner, cooldown);
            _actions.SetCooldown(action.Owner, cooldown);
        }
    }

    private void OnReflectHitscan(Entity<XenomorphBulwarkComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected || !ent.Comp.Reflecting || !ent.Comp.Encased || !_mobs.IsAlive(ent)) return;
        var incoming = (-args.Direction).ToWorldAngle().GetCardinalDir();
        var facing = _transform.GetWorldRotation(ent).GetCardinalDir();
        var chance = incoming == facing ? .8f : incoming == facing.GetOpposite() ? .3f : .65f;
        if (!_random.Prob(chance)) return;
        args.Direction = _random.NextAngle(-Angle.FromDegrees(90), Angle.FromDegrees(90)).Opposite().RotateVec(args.Direction);
        args.Reflected = true;
    }

    private void OnReflect(Entity<XenomorphBulwarkComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || !ent.Comp.Reflecting || !ent.Comp.Encased || !_mobs.IsAlive(ent)
            || HasComp<XenomorphSpitProjectileComponent>(args.ProjUid))
            return;
        var target = _transform.GetMapCoordinates(ent);
        var projectile = _transform.GetMapCoordinates(args.ProjUid);
        if (target.MapId != projectile.MapId)
            return;
        var facing = _transform.GetWorldRotation(ent).GetCardinalDir();
        var incoming = (projectile.Position - target.Position).ToWorldAngle().GetCardinalDir();
        var chance = incoming == facing ? 0.8f : incoming == facing.GetOpposite() ? 0.3f : 0.65f;
        if (!_random.Prob(chance) || !TryComp<PhysicsComponent>(args.ProjUid, out var physics))
            return;
        var velocity = _physics.GetMapLinearVelocity(args.ProjUid, component: physics);
        var reflected = (_random.NextAngle(-Angle.FromDegrees(90), Angle.FromDegrees(90)).Opposite()).RotateVec(velocity);
        _physics.SetLinearVelocity(args.ProjUid, reflected, body: physics);
        _transform.SetWorldRotation(args.ProjUid, reflected.ToWorldAngle() + args.Component.Angle);
        args.Component.Shooter = ent;
        args.Component.Weapon = ent;
        Dirty(args.ProjUid, args.Component);
        args.Cancelled = true;
    }

    private void OnMobState(Entity<XenomorphBulwarkComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            SetEncased(ent, false);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphBulwarkComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (comp.Reflecting && _timing.CurTime >= comp.ReflectExpiresAt)
                StopReflecting((uid, comp), false);
    }
}
