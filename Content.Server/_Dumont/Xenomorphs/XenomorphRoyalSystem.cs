using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Body.Systems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphQueenVolleyComponent : Component
{
    public HashSet<EntityUid> Hit = new();
}

[RegisterComponent]
public sealed partial class XenomorphQueenHealingComponent : Component
{
    public readonly List<(FixedPoint2 Amount, int Charges, TimeSpan Next)> Heals = new();
}

public sealed partial class XenomorphRoyalSystem : EntitySystem
{
    [Dependency] private Content.Shared.Camera.SharedCameraRecoilSystem _camera = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private Content.Shared.Popups.SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<Entity<MobStateComponent>> _victims = new();
    private readonly HashSet<Entity<XenomorphComponent>> _allies = new();
    private readonly Queue<(EntityUid User, EntityUid Target)> _guts = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRoyalComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenScreechActionEvent>(OnScreech);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenSpitActionEvent>(OnSpit);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenGutActionEvent>(OnGut);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenGutDoAfterEvent>(OnGutted);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenHealActionEvent>(OnHeal);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphQueenPlasmaActionEvent>(OnPlasma);
        SubscribeLocalEvent<XenomorphQueenVolleyComponent, ProjectileHitEvent>(OnVolleyHit, before: new[] { typeof(XenomorphSpitSystem) });
        SubscribeLocalEvent<XenomorphQueenScreechBlindComponent, ComponentRemove>(OnBlindRemoved);
        InitializeHive();
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanConsciouslyPerformAction(uid)
        && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting);
    private bool Attached(EntityUid uid) => TryComp<XenomorphOvipositorComponent>(uid, out var ovi) && ovi.Attached;
    private bool Sister(EntityUid user, EntityUid target) => HasComp<XenomorphLineageComponent>(target)
        && HasComp<XenomorphComponent>(target) && !_mobs.IsDead(target)
        && Transform(user).MapID == Transform(target).MapID;

    private void OnInit(Entity<XenomorphRoyalComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.MatureAt = _timing.CurTime + ent.Comp.MaturationDelay;
        RemCompDeferred<Content.Server._White.Xenomorphs.Evolution.XenomorphEvolutionComponent>(ent);
        UpdateOvipositorActions(ent);
    }

    private void OnScreech(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenScreechActionEvent args)
    {
        if (args.Handled || !ent.Comp.Mature || !Ready(ent) || !_plasma.ChangePlasmaAmount(ent, -250)) return;
        _victims.Clear();
        _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(ent), 7, _victims);
        foreach (var victim in _victims)
        {
            if (HasComp<XenomorphComponent>(victim) || _mobs.IsDead(victim)
                || !_interaction.InRangeUnobstructed(ent.Owner, victim.Owner, 7, collisionMask: Content.Shared.Physics.CollisionGroup.Impassable)) continue;
            var duration = TimeSpan.FromSeconds(8);
            var speed = TryComp<MovementSpeedModifierComponent>(victim, out var movement) ? Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) : 4;
            _slow.TryUpdateMovementSpeedModDuration(victim, "XenomorphStompSlowdown", duration, 1 / (1 + speed * 1.4f));
            _jitter.DoJitter(victim, duration, true, 3, 8);
            _camera.KickCamera(victim, new Vector2(3, 3));
            var blind = EnsureComp<XenomorphQueenScreechBlindComponent>(victim);
            blind.Until = _timing.CurTime + duration;
            if (!HasComp<DeafnessComponent>(victim)) { EnsureComp<DeafnessComponent>(victim); blind.AddedDeafness = true; }
            Dirty(victim, blind);
        }
        Spawn("DumontXenomorphQueenScreech", _transform.GetMoverCoordinates(ent));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg", AudioParams.Default.WithVolume(-7)), ent);
        args.Handled = true;
    }

    private void OnBlindRemoved(Entity<XenomorphQueenScreechBlindComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.AddedDeafness) RemCompDeferred<DeafnessComponent>(ent);
    }

    private void OnSpit(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenSpitActionEvent args)
    {
        if (args.Handled || !ent.Comp.Mature || !Ready(ent)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var delta = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(delta.LengthSquared()) || delta.LengthSquared() < .001f
            || !_plasma.ChangePlasmaAmount(ent, -40)) return;
        var hit = new HashSet<EntityUid>();
        for (var i = -1; i <= 1; i++)
        {
            var projectile = Spawn("DumontXenomorphQueenSpit", _transform.GetMoverCoordinates(ent));
            EnsureComp<XenomorphQueenVolleyComponent>(projectile).Hit = hit;
            Comp<XenomorphSpitProjectileComponent>(projectile).Origin = origin;
            _guns.ShootProjectile(projectile, Angle.FromDegrees(i * 15).RotateVec(delta), Vector2.Zero, ent, ent, 40);
        }
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphSpit", AudioParams.Default.WithVolume(-5)), ent);
        args.Handled = true;
    }

    private void OnVolleyHit(Entity<XenomorphQueenVolleyComponent> ent, ref ProjectileHitEvent args)
    {
        if (!ent.Comp.Hit.Add(args.Target))
        {
            args.Damage = new DamageSpecifier();
            if (TryComp<XenomorphSpitProjectileComponent>(ent, out var spit)) spit.Suppressed = true;
        }
    }

    private bool CanGut(EntityUid user, EntityUid target) => Ready(user) && !Attached(user) && user != target
        && !HasComp<XenomorphComponent>(target) && HasComp<BodyComponent>(target)
        && _interaction.InRangeUnobstructed(user, target, 2) && _plasma.HasPlasma(user, 200);
    private void OnGut(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenGutActionEvent args)
    {
        if (args.Handled || !CanGut(ent, args.Target)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 8,
            new XenomorphQueenGutDoAfterEvent(), ent, args.Target)
        { NeedHand = false, BreakOnMove = true, CancelDuplicate = false, BlockDuplicate = true, DistanceThreshold = 2 });
        if (args.Handled)
        {
            _jitter.DoJitter(args.Target, TimeSpan.FromSeconds(8), true, 14, 5);
            _popup.PopupEntity(Loc.GetString("xenomorph-queen-gut-start"), args.Target, args.Target, Content.Shared.Popups.PopupType.LargeCaution);
        }
    }
    private void OnGutted(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenGutDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target || !CanGut(ent, target)) return;
        args.Handled = true;
        _guts.Enqueue((ent, target));
    }

    private void OnHeal(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenHealActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Attached(ent) || !CanCommandAt(ent, args.Target)) return;
        _allies.Clear();
        _lookup.GetEntitiesInRange(args.Target, 4, _allies);
        var recipients = _allies.Where(x => Sister(ent, x) && !OnFire(x)).ToArray();
        if (recipients.Length == 0 || !_plasma.ChangePlasmaAmount(ent, -600)) return;
        foreach (var sister in recipients)
        {
            if (!_thresholds.TryGetThresholdForState(sister, MobState.Critical, out var threshold)) continue;
            EnsureComp<XenomorphQueenHealingComponent>(sister).Heals.Add((threshold!.Value * .03f, 10, _timing.CurTime + TimeSpan.FromSeconds(2)));
            Spawn("DumontXenomorphQueenHealEffect", new Robust.Shared.Map.EntityCoordinates(sister, Vector2.Zero));
        }
        args.Handled = true;
    }

    private bool OnFire(EntityUid uid) => TryComp<Content.Shared.Atmos.Components.FlammableComponent>(uid, out var fire) && fire.OnFire;
    private void Heal(EntityUid uid, FixedPoint2 amount)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable)) return;
        var damage = new DamageSpecifier();
        foreach (var (type, value) in damageable.Damage.DamageDict)
        {
            if (value <= 0 || amount <= 0) continue;
            var heal = FixedPoint2.Min(value, amount);
            damage.DamageDict[type] = -heal;
            amount -= heal;
        }
        _damage.TryChangeDamage(uid, damage, ignoreResistances: true, interruptsDoAfters: false);
    }

    private void OnPlasma(Entity<XenomorphRoyalComponent> ent, ref XenomorphQueenPlasmaActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Attached(ent) || args.Target == ent.Owner || !Sister(ent, args.Target)
            || !CanCommandAt(ent, Transform(args.Target).Coordinates)
            || !TryComp<Content.Shared._White.Xenomorphs.Plasma.Components.PlasmaVesselComponent>(args.Target, out var plasma)
            || plasma.Plasma >= plasma.MaxPlasma || !_plasma.ChangePlasmaAmount(ent, -400)) return;
        _plasma.ChangePlasmaAmount(args.Target, plasma.MaxPlasma * .75f);
        Spawn("DumontXenomorphPlasmaTransferEffect", new Robust.Shared.Map.EntityCoordinates(args.Target, Vector2.Zero));
        args.Handled = true;
    }

    public void UpdateOvipositorActions(EntityUid uid)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphQueenHealActionEvent or XenomorphQueenPlasmaActionEvent)
                _actions.SetEnabled(action.Owner, Attached(uid));
    }

    public override void Update(float frameTime)
    {
        while (_guts.TryDequeue(out var gut))
        {
            if (!Exists(gut.User) || !Exists(gut.Target) || !CanGut(gut.User, gut.Target) || !_plasma.ChangePlasmaAmount(gut.User, -200)) continue;
            _body.GibBody(gut.Target, true);
            _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), gut.User);
            foreach (var action in _actions.GetActions(gut.User))
                if (_actions.GetEvent(action) is XenomorphQueenGutActionEvent) _actions.SetCooldown(action.Owner, TimeSpan.FromMinutes(15));
        }
        var projectiles = EntityQueryEnumerator<XenomorphQueenVolleyComponent, XenomorphSpitProjectileComponent>();
        while (projectiles.MoveNext(out var projectile, out _, out var spit))
            if (spit.Origin is { } origin && (_transform.GetMapCoordinates(projectile).Position - origin.Position).LengthSquared() > 100) QueueDel(projectile);
        var queens = EntityQueryEnumerator<XenomorphRoyalComponent>();
        while (queens.MoveNext(out var uid, out var queen))
        {
            if (!queen.Mature && queen.MatureAt <= _timing.CurTime && _mobs.IsAlive(uid))
            {
                queen.Mature = true;
                _thresholds.SetMobStateThreshold(uid, 1300, MobState.Dead);
                _thresholds.SetMobStateThreshold(uid, 1200, MobState.Critical);
                _actions.AddAction(uid, "ActionXenomorphQueenScreech");
                _actions.AddAction(uid, "ActionXenomorphQueenSpit");
                Dirty(uid, queen);
                _popup.PopupEntity(Loc.GetString("xenomorph-queen-matured"), uid, uid);
            }
        }
        var blindQuery = EntityQueryEnumerator<XenomorphQueenScreechBlindComponent>();
        while (blindQuery.MoveNext(out var uid, out var blind))
            if (blind.Until <= _timing.CurTime) RemCompDeferred<XenomorphQueenScreechBlindComponent>(uid);
        var heals = EntityQueryEnumerator<XenomorphQueenHealingComponent>();
        while (heals.MoveNext(out var uid, out var healing))
        {
            if (_mobs.IsDead(uid) || OnFire(uid)) { healing.Heals.Clear(); continue; }
            for (var i = healing.Heals.Count - 1; i >= 0; i--)
            {
                var h = healing.Heals[i];
                if (h.Next > _timing.CurTime) continue;
                Heal(uid, h.Amount);
                if (h.Charges <= 1) healing.Heals.RemoveAt(i);
                else healing.Heals[i] = (h.Amount, h.Charges - 1, _timing.CurTime + TimeSpan.FromSeconds(2));
            }
            if (healing.Heals.Count == 0) RemCompDeferred<XenomorphQueenHealingComponent>(uid);
        }
        UpdateHive();
    }
}
