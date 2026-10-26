using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphVenatorProjectileComponent : Component
{
    [DataField] public bool Blob;
    public bool Pooled;
    public EntityUid? Shooter;
    public MapCoordinates Origin;
    public MapCoordinates Destination;
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphVenatorPoolComponent : Component
{
    [DataField] public bool Blob;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Until;
    public EntityUid? Shooter;
    public readonly HashSet<EntityUid> Hit = new();
}

public sealed partial class XenomorphVenatorSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private XenomorphSpitterSystem _spitter = default!;
    [Dependency] private XenomorphPraetorianSystem _praetorian = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    private readonly HashSet<Entity<MobStateComponent>> _victims = new();
    private static readonly Vector2[] Cross = { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };
    private static readonly Vector2[] Diagonal = { new(-1, -1), new(-1, 1), new(1, -1), new(1, 1) };

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVenatorComponent, XenomorphStoreAcidActionEvent>(OnStore);
        SubscribeLocalEvent<XenomorphVenatorComponent, XenomorphVenatorSpitActionEvent>(OnSpit);
        SubscribeLocalEvent<XenomorphVenatorProjectileComponent, ProjectileHitEvent>(OnHit, before: new[] { typeof(XenomorphSpitSystem) });
    }
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid);
    private void OnStore(Entity<XenomorphVenatorComponent> ent, ref XenomorphStoreAcidActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        if (ent.Comp.StoreLockedUntil > _timing.CurTime)
        {
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-venator-store-locked"), ent, ent);
            return;
        }
        if (ent.Comp.AcidCharges >= 5 || !_plasma.HasPlasma(ent, 150)) return;
        _plasma.ChangePlasmaAmount(ent, -150);
        ent.Comp.AcidCharges++;
        Dirty(ent);
        _armor.RefreshAlert(ent);
        _actions.SetToggled(args.Action.Owner, true);
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-venator-stored", ("amount", ent.Comp.AcidCharges)), ent, ent);
        args.Handled = true;
    }
    private void OnSpit(Entity<XenomorphVenatorComponent> ent, ref XenomorphVenatorSpitActionEvent args)
    {
        var cost = args.Blob ? 65 : args.Breath ? 55 : 45;
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, cost)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        var distance = direction.Length();
        if (origin.MapId != target.MapId || !float.IsFinite(distance) || distance < .01f || distance > 8.01f) return;
        _plasma.ChangePlasmaAmount(ent, -cost);
        for (var i = 0; i < (args.Breath ? 5 : 1); i++)
        {
            var offset = i == 0 ? Angle.Zero : _random.NextAngle(-Angle.FromDegrees(12.5), Angle.FromDegrees(12.5));
            var projectile = Spawn(args.Blob ? "DumontXenomorphVenatorAcidBlob" : args.Breath ? "DumontXenomorphVenatorBreath" : "DumontXenomorphVenatorSpit", _transform.GetMoverCoordinates(ent));
            var comp = Comp<XenomorphVenatorProjectileComponent>(projectile);
            comp.Shooter = ent;
            comp.Origin = origin;
            comp.Destination = new MapCoordinates(origin.Position + offset.RotateVec(direction), origin.MapId);
            Comp<XenomorphSpitProjectileComponent>(projectile).Origin = origin;
            _guns.ShootProjectile(projectile, offset.RotateVec(direction), Vector2.Zero, ent, ent, args.Blob ? 7.5f : args.Breath ? 22 : 24);
        }
        ent.Comp.StoreLockedUntil = _timing.CurTime + TimeSpan.FromSeconds(4);
        var delay = TimeSpan.FromSeconds(ent.Comp.AcidCharges > 0 ? 1 : 11);
        if (ent.Comp.AcidCharges > 0) ent.Comp.AcidCharges--;
        Dirty(ent);
        _armor.RefreshAlert(ent);
        foreach (var action in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(action) is XenomorphVenatorSpitActionEvent)
            {
                _actions.SetUseDelay(action.Owner, delay);
                _actions.SetCooldown(action.Owner, delay);
            }
            else if (_actions.GetEvent(action) is XenomorphStoreAcidActionEvent)
                _actions.SetToggled(action.Owner, ent.Comp.AcidCharges > 0);
        }
        _audio.PlayPvs(args.Blob ? new SoundPathSpecifier("/Audio/_RMC14/Xeno/blobattack.ogg")
            : new SoundCollectionSpecifier("DumontXenomorphSpit", AudioParams.Default.WithVolume(-10)), ent);
        args.Handled = true;
    }
    private void OnHit(Entity<XenomorphVenatorProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if ((_transform.GetMapCoordinates(ent).Position - ent.Comp.Origin.Position).LengthSquared() > 64.01f)
        {
            args.Damage = new DamageSpecifier();
            return;
        }
        if (HasComp<XenomorphComponent>(args.Target)) return;
        var wasAcided = HasComp<XenomorphAcidedComponent>(args.Target);
        SpawnPools(ent, Transform(args.Target).Coordinates, args.Target);
        if (!Victim(args.Target)) return;
        if (!wasAcided)
        {
            _spitter.ApplyAcid(args.Target);
            if (TryComp<XenomorphAcidedComponent>(args.Target, out var acid))
            {
                acid.Duration = TimeSpan.FromSeconds(12);
                acid.ExpiresAt = _timing.CurTime + acid.Duration;
                acid.ArmorPenetration = .3f;
            }
        }
        if (ent.Comp.Blob && wasAcided) _spitter.EnhanceAcid(args.Target);
    }
    private void SpawnPools(Entity<XenomorphVenatorProjectileComponent> ent, EntityCoordinates coordinates, EntityUid? directTarget = null)
    {
        if (ent.Comp.Pooled) return;
        ent.Comp.Pooled = true;
        SpawnPool(ent, coordinates, directTarget);
        if (!ent.Comp.Blob) return;
        foreach (var offset in _random.Prob(.5f) ? Cross : Diagonal)
        {
            var point = coordinates.Offset(offset);
            if (_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(coordinates), _transform.ToMapCoordinates(point), 2))
                SpawnPool(ent, point, directTarget);
        }
    }
    private void SpawnPool(Entity<XenomorphVenatorProjectileComponent> ent, EntityCoordinates coordinates, EntityUid? directTarget)
    {
        var pool = Spawn(ent.Comp.Blob ? "DumontXenomorphVenatorBlobPool" : "DumontXenomorphVenatorPool", coordinates);
        var comp = Comp<XenomorphVenatorPoolComponent>(pool);
        comp.Shooter = ent.Comp.Shooter;
        comp.Until = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Blob ? 1 : .6);
        if (directTarget is { } target) comp.Hit.Add(target);
        if (Transform(pool).GridUid is { } grid)
        {
            var local = _transform.ToCoordinates(grid, _transform.GetMapCoordinates(pool));
            _atmos.HotspotExtinguish(grid, new Vector2i((int)MathF.Floor(local.X), (int)MathF.Floor(local.Y)));
        }
    }
    public override void Update(float frameTime)
    {
        var projectiles = EntityQueryEnumerator<XenomorphVenatorProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out var comp))
        {
            if (comp.Pooled) continue;
            var current = _transform.GetMapCoordinates(uid);
            var distance = (current.Position - comp.Origin.Position).LengthSquared();
            if (comp.Blob && distance >= (comp.Destination.Position - comp.Origin.Position).LengthSquared())
            {
                SpawnPools((uid, comp), _transform.ToCoordinates(Transform(uid).ParentUid, comp.Destination));
                QueueDel(uid);
            }
            else if (distance > 64) QueueDel(uid);
        }
        var pools = EntityQueryEnumerator<XenomorphVenatorPoolComponent>();
        while (pools.MoveNext(out var uid, out var pool))
        {
            if (_timing.CurTime >= pool.Until) continue;
            _victims.Clear();
            _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(uid), .5f, _victims);
            foreach (var target in _victims)
            {
                _fire.Extinguish(target);
                if (!Victim(target) || pool.Hit.Contains(target) || !_interaction.InRangeUnobstructed(uid, target.Owner, range: .75f)) continue;
                pool.Hit.Add(target);
                _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = pool.Blob ? 24 : 30 } }, origin: pool.Shooter is { } source && Exists(source) ? source : null);
                if (TerminatingOrDeleted(target)) continue;
                _spitter.ShowAcidSplash(target);
                _spitter.EnhanceAcid(target);
                if (pool.Blob && TryComp<XenomorphAcidStacksComponent>(target, out var stacks) && stacks.Current > 0)
                    _praetorian.ApplyStacks(target, 2, pool.Shooter);
            }
        }
    }
}
