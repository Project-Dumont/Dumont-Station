using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphPraetorianProjectileComponent : Component
{
    [DataField] public bool RewardShield;
    [DataField] public float Range = 7;
    public MapCoordinates Origin;
    public EntityUid? Burst;
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphAcidBallComponent : Component
{
    public EntityUid? Shooter;
    public MapCoordinates Origin;
    public float Range = 6;
    [DataField] public bool Armed;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExplodeAt;
}

[RegisterComponent]
public sealed partial class XenomorphAcidBallBurstComponent : Component
{
    public readonly Dictionary<EntityUid, int> Hits = new();
}

public sealed partial class XenomorphPraetorianSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private XenomorphSpitterSystem _spitter = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPraetorianComponent, XenomorphPraetorianSpitActionEvent>(OnSpit);
        SubscribeLocalEvent<XenomorphPraetorianComponent, XenomorphAcidBallActionEvent>(OnBall);
        SubscribeLocalEvent<XenomorphPraetorianComponent, XenomorphAcidBallDoAfterEvent>(OnBallPrepared);
        SubscribeLocalEvent<XenomorphPraetorianComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem) });
        SubscribeLocalEvent<XenomorphPraetorianProjectileComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphPraetorianProjectileComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphAcidBallComponent, ProjectileHitEvent>(OnBallHit);
        SubscribeLocalEvent<XenomorphAcidBallComponent, PreventCollideEvent>(OnBallCollide);
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid);

    public void ApplyStacks(EntityUid target, int amount, EntityUid? source = null)
    {
        if (!Victim(target)) return;
        var stacks = EnsureComp<XenomorphAcidStacksComponent>(target);
        stacks.Current = Math.Min(5, stacks.Current + amount);
        stacks.LastIncrement = _timing.CurTime;
        Dirty(target, stacks);
        if (stacks.Current < 5) return;
        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = 30 } }, origin: source);
        _spitter.ShowAcidSplash(target);
        RemComp<XenomorphAcidStacksComponent>(target);
    }

    private bool Direction(EntityUid uid, EntityCoordinates coordinates, out Vector2 direction)
    {
        var source = _transform.GetMapCoordinates(uid);
        var target = _transform.ToMapCoordinates(coordinates);
        direction = target.Position - source.Position;
        return source.MapId == target.MapId && float.IsFinite(direction.LengthSquared()) && direction.LengthSquared() > 0.01f;
    }

    private void OnSpit(Entity<XenomorphPraetorianComponent> ent, ref XenomorphPraetorianSpitActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Direction(ent, args.Target, out var direction) || !_plasma.ChangePlasmaAmount(ent, -25)) return;
        var projectile = Spawn("DumontXenomorphPraetorianSpit", _transform.GetMoverCoordinates(ent));
        Comp<XenomorphPraetorianProjectileComponent>(projectile).Origin = _transform.GetMapCoordinates(ent);
        _guns.ShootProjectile(projectile, direction, Vector2.Zero, ent, ent, 30);
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphSpit", AudioParams.Default.WithVolume(-10)), ent);
        args.Handled = true;
    }

    private void Preparing(Entity<XenomorphPraetorianComponent> ent, bool value)
    {
        ent.Comp.Preparing = value;
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphAcidBallActionEvent) _actions.SetToggled(action.Owner, value);
    }

    private void OnBall(Entity<XenomorphPraetorianComponent> ent, ref XenomorphAcidBallActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !Ready(ent) || !_plasma.HasPlasma(ent, 80) || !Direction(ent, args.Target, out _)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(1),
            new XenomorphAcidBallDoAfterEvent { Coordinates = GetNetCoordinates(args.Target) }, ent) { NeedHand = false });
        if (args.Handled) Preparing(ent, true);
    }

    private void OnBallPrepared(Entity<XenomorphPraetorianComponent> ent, ref XenomorphAcidBallDoAfterEvent args)
    {
        Preparing(ent, false);
        if (args.Cancelled || args.Handled || !Ready(ent) || !Direction(ent, GetCoordinates(args.Coordinates), out var direction)
            || !_plasma.ChangePlasmaAmount(ent, -80)) return;
        var projectile = Spawn("DumontXenomorphAcidBall", _transform.GetMoverCoordinates(ent));
        var comp = Comp<XenomorphAcidBallComponent>(projectile);
        comp.Shooter = ent;
        comp.Origin = _transform.GetMapCoordinates(ent);
        comp.Range = Math.Min(6, direction.Length());
        _guns.ShootProjectile(projectile, direction, Vector2.Zero, ent, ent, 15);
        var reduction = TryComp<XenomorphFruitBuffComponent>(ent, out var fruit) && _timing.CurTime < fruit.SporeUntil ? fruit.SporeReduction : 0;
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphAcidBallActionEvent)
                _actions.SetCooldown(action.AsNullable(), _timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(18 * (1 - reduction)));
        args.Handled = true;
    }

    private void OnCollide(Entity<XenomorphPraetorianProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity) || HasComp<MobStateComponent>(args.OtherEntity) && _mobs.IsDead(args.OtherEntity)) args.Cancelled = true;
        if (ent.Comp.Burst is { } burst && TryComp<XenomorphAcidBallBurstComponent>(burst, out var cluster)
            && cluster.Hits.TryGetValue(args.OtherEntity, out var hits) && hits >= 4) args.Cancelled = true;
    }

    private void OnHit(Entity<XenomorphPraetorianProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!Victim(args.Target)) return;
        if (ent.Comp.Burst is { } burst && TryComp<XenomorphAcidBallBurstComponent>(burst, out var cluster))
        {
            cluster.Hits.TryGetValue(args.Target, out var hits);
            if (hits >= 4) { args.Damage = new DamageSpecifier(); return; }
            cluster.Hits[args.Target] = hits + 1;
        }
        var shooter = Comp<ProjectileComponent>(ent).Shooter;
        ApplyStacks(args.Target, 1, shooter);
        _spitter.ShowAcidSplash(args.Target);
        if (ent.Comp.RewardShield && TryComp<XenomorphPraetorianComponent>(shooter, out var prae))
        {
            if (TryComp<XenomorphFruitBuffComponent>(shooter, out var fruit) && fruit.Shield > 0)
            {
                fruit.Shield = 0;
                if (Exists(fruit.ShieldEffect)) QueueDel(fruit.ShieldEffect);
                fruit.ShieldEffect = null;
            }
            prae.Shield = Math.Min(45, prae.Shield + 15);
            Dirty(shooter.Value, prae);
        }
    }

    private void OnDamage(Entity<XenomorphPraetorianComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Shield <= 0 || args.Damage.GetTotal() <= 0) return;
        args.Damage = new DamageSpecifier(args.Damage);
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0) continue;
            var absorbed = Content.Goobstation.Maths.FixedPoint.FixedPoint2.New(Math.Min(value.Float(), ent.Comp.Shield));
            ent.Comp.Shield -= absorbed.Float();
            args.Damage.DamageDict[type] -= absorbed;
        }
        Dirty(ent);
        if (ent.Comp.Shield <= 0) _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Bullets/shield_break_c1.ogg"), ent);
    }

    private void OnBallCollide(Entity<XenomorphAcidBallComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity)) args.Cancelled = true;
    }
    private void OnBallHit(Entity<XenomorphAcidBallComponent> ent, ref ProjectileHitEvent args) => Arm(ent);
    private void Arm(Entity<XenomorphAcidBallComponent> ent)
    {
        if (ent.Comp.Armed) return;
        ent.Comp.Armed = true;
        ent.Comp.ExplodeAt = _timing.CurTime + TimeSpan.FromSeconds(1);
        _physics.SetLinearVelocity(ent, Vector2.Zero);
    }
    private void Burst(Entity<XenomorphAcidBallComponent> ent)
    {
        var origin = _transform.GetMoverCoordinates(ent);
        var cluster = Spawn("DumontXenomorphAcidBallBurst", origin);
        for (var i = 0; i < 14; i++)
        {
            var projectile = Spawn("DumontXenomorphAcidBallSpit", origin);
            var comp = Comp<XenomorphPraetorianProjectileComponent>(projectile);
            comp.Origin = _transform.ToMapCoordinates(origin);
            comp.Burst = cluster;
            var angle = Angle.FromDegrees(_random.NextFloat(i * 360f / 14, (i + 1) * 360f / 14));
            var velocity = _random.NextVector2(2, 6);
            _guns.ShootProjectile(projectile, angle.ToVec(), velocity, ent.Comp.Shooter ?? ent.Owner, ent.Comp.Shooter, 20);
        }
        QueueDel(ent);
    }
    public override void Update(float frameTime)
    {
        var stacks = EntityQueryEnumerator<XenomorphAcidStacksComponent>();
        while (stacks.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.LastIncrement + TimeSpan.FromSeconds(5) || _timing.CurTime < comp.LastDecrement + TimeSpan.FromSeconds(4)) continue;
            if (--comp.Current <= 0) { RemCompDeferred<XenomorphAcidStacksComponent>(uid); continue; }
            comp.LastDecrement = _timing.CurTime;
            Dirty(uid, comp);
        }
        var projectiles = EntityQueryEnumerator<XenomorphPraetorianProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out var projectile))
        {
            var position = _transform.GetMapCoordinates(uid);
            if (position.MapId != projectile.Origin.MapId || (position.Position - projectile.Origin.Position).LengthSquared() >= projectile.Range * projectile.Range) QueueDel(uid);
        }
        var balls = EntityQueryEnumerator<XenomorphAcidBallComponent>();
        while (balls.MoveNext(out var uid, out var ball))
        {
            if (ball.Armed) { if (_timing.CurTime >= ball.ExplodeAt) Burst((uid, ball)); continue; }
            var position = _transform.GetMapCoordinates(uid);
            if (position.MapId != ball.Origin.MapId) { QueueDel(uid); continue; }
            if ((position.Position - ball.Origin.Position).LengthSquared() >= ball.Range * ball.Range) Arm((uid, ball));
        }
    }
}
