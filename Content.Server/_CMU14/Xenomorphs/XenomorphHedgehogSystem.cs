using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphShardVolley
{
    public readonly Dictionary<EntityUid, int> Hits = new();
}

[RegisterComponent]
public sealed partial class XenomorphShardProjectileComponent : Component
{
    [DataField] public float Range = 4;
    public MapCoordinates? Origin;
    public XenomorphShardVolley? Volley;
    public int HitLimit;
}

public sealed partial class XenomorphHedgehogSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHedgehogComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphHedgehogComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<XenomorphHedgehogComponent, XenomorphFireSpikesActionEvent>(OnFire);
        SubscribeLocalEvent<XenomorphHedgehogComponent, XenomorphSpikeShieldActionEvent>(OnShield);
        SubscribeLocalEvent<XenomorphHedgehogComponent, XenomorphSpikeShedActionEvent>(OnShed);
        SubscribeLocalEvent<XenomorphHedgehogComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphHedgehogComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<XenomorphHedgehogComponent, XenomorphShardsAlertEvent>(OnAlert);
        SubscribeLocalEvent<XenomorphHedgehogComponent, ProjectileReflectAttemptEvent>(OnIncoming);
        SubscribeLocalEvent<XenomorphHedgehogComponent, DamageChangedEvent>(OnInjured);
        SubscribeLocalEvent<XenomorphHedgehogComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem), typeof(XenomorphReaperSystem) });
        SubscribeLocalEvent<XenomorphShardProjectileComponent, MapInitEvent>(OnProjectileInit);
        SubscribeLocalEvent<XenomorphShardProjectileComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphShardProjectileComponent, ProjectileHitEvent>(OnHit);
    }

    private bool Friendly(EntityUid uid) => HasComp<XenomorphComponent>(uid) || HasComp<FaceHuggerComponent>(uid);
    private bool Nested(EntityUid uid) => TryComp<BuckleComponent>(uid, out var buckle) && buckle.BuckledTo is { } nest && HasComp<XenomorphNestComponent>(nest);
    private bool Enemy(EntityUid uid) => HasComp<MobStateComponent>(uid) && !Friendly(uid) && !_mobs.IsDead(uid) && !Nested(uid) && !_containers.IsEntityInContainer(uid);
    private bool CanUse(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid) && !_containers.IsEntityInContainer(uid)
        && !(TryComp<XenomorphRestComponent>(uid, out var rest) && rest.Resting);

    private void OnInit(Entity<XenomorphHedgehogComponent> ent, ref MapInitEvent args) => Refresh(ent);

    private void Refresh(Entity<XenomorphHedgehogComponent> ent)
    {
        Dirty(ent);
        _alerts.ShowAlert(ent, "DumontXenomorphShards", (short)(18 - Math.Clamp(ent.Comp.Shards * 18 / Math.Max(1, ent.Comp.MaxShards), 0, 18)));
        _armor.RefreshAlert(ent);
    }

    private void Gain(Entity<XenomorphHedgehogComponent> ent, int amount)
    {
        if (ent.Comp.ShedUntil > _timing.CurTime) return;
        ent.Comp.Shards = Math.Clamp(ent.Comp.Shards + amount, 0, ent.Comp.MaxShards);
        Refresh(ent);
    }

    private bool Spend(Entity<XenomorphHedgehogComponent> ent, int cost)
    {
        if (ent.Comp.Shards < cost)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-hedgehog-no-shards", ("amount", cost)), ent, ent);
            return false;
        }
        ent.Comp.Shards -= cost;
        Refresh(ent);
        return true;
    }

    private void OnAlert(Entity<XenomorphHedgehogComponent> ent, ref XenomorphShardsAlertEvent args)
    {
        _popup.PopupEntity(Loc.GetString("xenomorph-hedgehog-shards", ("amount", ent.Comp.Shards), ("max", ent.Comp.MaxShards)), ent, ent);
        args.Handled = true;
    }

    private void OnMelee(Entity<XenomorphHedgehogComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner) return;
        var target = args.HitEntities.FirstOrDefault(Enemy);
        if (!Enemy(target)) return;
        Gain(ent, 15);
        var speed = TryComp<MovementSpeedModifierComponent>(target, out var movement)
            ? Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) : 3.5f;
        _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(3.5),
            1 / (1 + speed * 0.2f));
    }

    private void OnRejuvenate(Entity<XenomorphHedgehogComponent> ent, ref RejuvenateEvent args) => Gain(ent, ent.Comp.MaxShards);

    private void OnIncoming(Entity<XenomorphHedgehogComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || args.Component.Damage.GetTotal() <= 0 || args.Component.Shooter is { } shooter && Friendly(shooter)) return;
        ent.Comp.IncomingAt = _timing.CurTime;
        ent.Comp.IncomingShooter = args.Component.Shooter;
        ent.Comp.IncomingProjectile = true;
        if (ent.Comp.Shield > 0 && ent.Comp.ShieldUntil > _timing.CurTime)
            Fire(ent, Vector2.UnitX, 7, 360, 15, true, 6);
    }

    private void OnInjured(Entity<XenomorphHedgehogComponent> ent, ref DamageChangedEvent args)
    {
        if (!ent.Comp.IncomingProjectile || ent.Comp.IncomingAt != _timing.CurTime || args.Origin != ent.Comp.IncomingShooter || !args.DamageIncreased) return;
        ent.Comp.IncomingProjectile = false;
        Gain(ent, 10);
    }

    private void OnFire(Entity<XenomorphHedgehogComponent> ent, ref XenomorphFireSpikesActionEvent args)
    {
        if (args.Handled || !CanUse(ent)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < 0.0001f || !Spend(ent, 75)) return;
        Fire(ent, direction, 8, 60, 20, false, 0);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/spike_spray.ogg"), ent);
        args.Handled = true;
    }

    private void Fire(EntityUid uid, Vector2 direction, int count, float deviation, float speed, bool shortRange, int limit)
    {
        var origin = _transform.GetMapCoordinates(uid);
        var volley = limit > 0 ? new XenomorphShardVolley() : null;
        for (var i = 0; i < count; i++)
        {
            var angle = i == 0 ? Angle.Zero : _random.NextAngle(-Angle.FromDegrees(deviation / 2), Angle.FromDegrees(deviation / 2));
            var projectile = Spawn(shortRange ? "DumontXenomorphSpikeShort" : "DumontXenomorphSpike", _transform.GetMoverCoordinates(uid));
            var shard = Comp<XenomorphShardProjectileComponent>(projectile);
            shard.Origin = origin;
            shard.Volley = volley;
            shard.HitLimit = limit;
            _guns.ShootProjectile(projectile, angle.RotateVec(direction), Vector2.Zero, uid, uid, speed);
        }
    }

    private void OnProjectileInit(Entity<XenomorphShardProjectileComponent> ent, ref MapInitEvent args)
        => ent.Comp.Origin = _transform.GetMapCoordinates(ent);

    private bool HitLimitReached(XenomorphShardProjectileComponent shard, EntityUid target)
        => HasComp<MobStateComponent>(target) && shard.Volley != null && shard.Volley.Hits.GetValueOrDefault(target) >= shard.HitLimit;

    private void OnCollide(Entity<XenomorphShardProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (Friendly(args.OtherEntity) || Nested(args.OtherEntity) || HitLimitReached(ent.Comp, args.OtherEntity)) args.Cancelled = true;
    }

    private void OnHit(Entity<XenomorphShardProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        var origin = ent.Comp.Origin;
        var current = _transform.GetMapCoordinates(ent);
        if (Friendly(args.Target) || Nested(args.Target) || HitLimitReached(ent.Comp, args.Target)
            || origin is { } from && (current.MapId != from.MapId || (current.Position - from.Position).LengthSquared() > ent.Comp.Range * ent.Comp.Range))
        {
            args.Damage = new DamageSpecifier();
            return;
        }
        if (HasComp<MobStateComponent>(args.Target) && ent.Comp.Volley is { } volley) volley.Hits[args.Target] = volley.Hits.GetValueOrDefault(args.Target) + 1;
        if (!Enemy(args.Target)) return;
        var speed = TryComp<MovementSpeedModifierComponent>(args.Target, out var movement) ? Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) : 3.5f;
        _slow.TryUpdateMovementSpeedModDuration(args.Target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(5), 1 / (1 + speed * 0.2f));
    }

    private void Toggle<T>(EntityUid uid, bool active) where T : BaseActionEvent
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is T) _actions.SetToggled(action.AsNullable(), active);
    }

    private void OnShield(Entity<XenomorphHedgehogComponent> ent, ref XenomorphSpikeShieldActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !Spend(ent, 150)) return;
        ClearShield(ent);
        ent.Comp.Shield = 500;
        ent.Comp.ShieldUntil = _timing.CurTime + TimeSpan.FromSeconds(2);
        ent.Comp.ShieldEffect = Spawn("DumontXenomorphSpikeShieldEffect", new EntityCoordinates(ent, Vector2.Zero));
        _appearance.SetData(ent.Comp.ShieldEffect.Value, XenomorphHedgehogVisuals.ShieldFraction, 1f);
        Toggle<XenomorphSpikeShieldActionEvent>(ent, true);
        args.Handled = true;
    }

    private void OnDamage(Entity<XenomorphHedgehogComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.ShieldUntil > _timing.CurTime && ent.Comp.Shield > 0)
        {
            args.Damage = new DamageSpecifier(args.Damage);
            foreach (var (type, value) in args.Damage.DamageDict)
            {
                if (value <= 0) continue;
                var absorbed = Math.Min(value.Float(), ent.Comp.Shield);
                ent.Comp.Shield -= absorbed;
                args.Damage.DamageDict[type] -= FixedPoint2.New(absorbed);
            }
            if (ent.Comp.ShieldEffect is { } effect)
                _appearance.SetData(effect, XenomorphHedgehogVisuals.ShieldFraction, ent.Comp.Shield / 500f);
            if (ent.Comp.Shield <= 0) ClearShield(ent);
        }
        if (args.Damage.GetTotal() <= 0) ent.Comp.IncomingProjectile = false;
    }

    private void ClearShield(Entity<XenomorphHedgehogComponent> ent)
    {
        ent.Comp.Shield = 0;
        if (ent.Comp.ShieldEffect is { } effect) QueueDel(effect);
        ent.Comp.ShieldEffect = null;
        Toggle<XenomorphSpikeShieldActionEvent>(ent, false);
    }

    private void OnShed(Entity<XenomorphHedgehogComponent> ent, ref XenomorphSpikeShedActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !Spend(ent, 50)) return;
        ent.Comp.Shards = 0;
        ent.Comp.ShedUntil = _timing.CurTime + TimeSpan.FromSeconds(30);
        EnsureComp<XenomorphSweepingComponent>(ent);
        Fire(ent, Vector2.UnitX, 40, 360, 20, false, 6);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/spike_spray.ogg"), ent);
        Refresh(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        Toggle<XenomorphSpikeShedActionEvent>(ent, true);
        _popup.PopupEntity(Loc.GetString("xenomorph-hedgehog-shed"), ent, ent);
        args.Handled = true;
    }

    private void OnRemove(Entity<XenomorphHedgehogComponent> ent, ref ComponentRemove args)
    {
        ClearShield(ent);
        Toggle<XenomorphSpikeShedActionEvent>(ent, false);
        _alerts.ClearAlert(ent, "DumontXenomorphShards");
        ent.Comp.Shards = 0;
        _armor.RefreshAlert(ent);
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphHedgehogComponent>();
        while (query.MoveNext(out var uid, out var hedgehog))
        {
            if (hedgehog.Shield > 0 && now >= hedgehog.ShieldUntil) ClearShield((uid, hedgehog));
            if (hedgehog.ShedUntil != TimeSpan.Zero && now >= hedgehog.ShedUntil)
            {
                hedgehog.ShedUntil = TimeSpan.Zero;
                Toggle<XenomorphSpikeShedActionEvent>(uid, false);
                _movement.RefreshMovementSpeedModifiers(uid);
                Refresh((uid, hedgehog));
                _popup.PopupEntity(Loc.GetString("xenomorph-hedgehog-regrowth"), uid, uid);
            }
            if (now < hedgehog.NextGain || _mobs.IsDead(uid)) continue;
            hedgehog.NextGain = now + TimeSpan.FromSeconds(2);
            Gain((uid, hedgehog), 5);
        }
        var projectiles = EntityQueryEnumerator<XenomorphShardProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out var shard))
        {
            if (shard.Origin is not { } origin) continue;
            var current = _transform.GetMapCoordinates(uid);
            if (current.MapId != origin.MapId || (current.Position - origin.Position).LengthSquared() >= shard.Range * shard.Range) QueueDel(uid);
        }
    }
}
