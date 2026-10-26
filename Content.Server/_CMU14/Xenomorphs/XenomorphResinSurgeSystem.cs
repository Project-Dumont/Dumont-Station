using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Spawners.Components;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Explosion;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphHardyWeedComponent : Component;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphSurgeReinforcedComponent : Component
{
    [DataField] public FixedPoint2 Remaining = 6000;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Until;
    public EntityUid? Effect;
}

[RegisterComponent]
public sealed partial class XenomorphStickyResinComponent : Component;

public sealed partial class XenomorphResinSurgeSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinSurgeComponent, XenomorphResinSurgeActionEvent>(OnSurge);
        SubscribeLocalEvent<XenomorphResinSurgeComponent, XenomorphResinSurgeDoAfterEvent>(OnSurged);
        SubscribeLocalEvent<XenomorphResinSurgeComponent, XenomorphPlantHardyWeedsActionEvent>(OnPlantHardy);
        SubscribeLocalEvent<XenomorphHardyWeedComponent, MapInitEvent>(OnHardyInit);
        SubscribeLocalEvent<XenomorphSurgeReinforcedComponent, DamageModifyEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphSurgeReinforcedComponent, BeforeExplodeEvent>(OnExplosion);
        SubscribeLocalEvent<XenomorphSurgeReinforcedComponent, ComponentRemove>(OnReinforceRemoved);
    }

    private EntityCoordinates Snap(EntityCoordinates coordinates)
    {
        var grid = Transform(coordinates.EntityId).GridUid ?? coordinates.EntityId;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
    }

    private bool CanAct(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanConsciouslyPerformAction(uid)
        && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting);

    private bool ClearTile(EntityCoordinates coordinates)
    {
        var tile = _turf.GetTileRef(coordinates);
        return tile is { } floor && !floor.Tile.IsEmpty && !_turf.IsTileBlocked(floor,
            CollisionGroup.Impassable | CollisionGroup.MidImpassable | CollisionGroup.HighImpassable);
    }

    private bool HasWeeds(EntityCoordinates coordinates)
    {
        foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates))
            if (HasComp<XenomorphWeedWallSpreadComponent>(uid)) return true;
        return false;
    }

    private void OnHardyInit(Entity<XenomorphHardyWeedComponent> ent, ref MapInitEvent args)
    {
        foreach (var uid in _lookup.GetEntitiesIntersecting(Transform(ent).Coordinates))
            if (uid != ent.Owner && HasComp<XenomorphWeedWallSpreadComponent>(uid) && !HasComp<XenomorphHardyWeedComponent>(uid)) QueueDel(uid);
    }

    private void OnPlantHardy(Entity<XenomorphResinSurgeComponent> ent, ref XenomorphPlantHardyWeedsActionEvent args)
    {
        if (args.Handled || !CanAct(ent)) return;
        var coordinates = Snap(_transform.GetMoverCoordinates(ent));
        if (!ClearTile(coordinates)) return;
        foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates))
            if (HasComp<XenomorphHardyWeedComponent>(uid) && HasComp<AreaSpawnerComponent>(uid)) return;
        if (!_plasma.ChangePlasmaAmount(ent, -125)) return;
        Spawn("DumontXenomorphHardyWeedNode", coordinates);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_resin_build1.ogg"), ent);
        args.Handled = true;
    }

    private void Cooldown(EntityUid user, double seconds)
    {
        foreach (var action in _actions.GetActions(user))
            if (_actions.GetEvent(action) is XenomorphResinSurgeActionEvent)
            {
                _actions.SetUseDelay(action.AsNullable(), TimeSpan.FromSeconds(seconds));
                _actions.SetCooldown(action.AsNullable(), _timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(seconds));
            }
    }

    private void SetBusy(Entity<XenomorphResinSurgeComponent> ent, bool busy)
    {
        ent.Comp.Busy = busy;
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphResinSurgeActionEvent) _actions.SetToggled(action.Owner, busy);
    }

    private void OnSurge(Entity<XenomorphResinSurgeComponent> ent, ref XenomorphResinSurgeActionEvent args)
    {
        if (args.Handled || ent.Comp.Busy || !CanAct(ent)) return;
        var coordinates = Snap(args.Target);
        var target = args.Entity;
        if (!_interaction.InRangeUnobstructed(ent, coordinates, 7, predicate: uid => uid == target)) return;
        if (target == null)
            foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates))
                if (HasComp<XenomorphSurgeReinforcableComponent>(uid) || HasComp<XenomorphFruitComponent>(uid)) { target = uid; break; }
        if (target is { } reinforce && HasComp<XenomorphSurgeReinforcableComponent>(reinforce))
        {
            if (!_plasma.ChangePlasmaAmount(ent, -75)) return;
            args.Handled = true;
            if (HasComp<XenomorphSurgeReinforcedComponent>(reinforce)) { Cooldown(ent, 5); return; }
            var shield = EnsureComp<XenomorphSurgeReinforcedComponent>(reinforce);
            shield.Until = _timing.CurTime + TimeSpan.FromSeconds(15);
            shield.Effect = Spawn(HasComp<DoorComponent>(reinforce) ? "DumontXenomorphSurgeDoorEffect" : "DumontXenomorphSurgeWallEffect", new EntityCoordinates(reinforce, Vector2.Zero));
            Cooldown(ent, 10);
            return;
        }
        if (target is { } fruitUid && TryComp<XenomorphFruitComponent>(fruitUid, out var fruit))
        {
            if (!_plasma.ChangePlasmaAmount(ent, -75)) return;
            args.Handled = true;
            if (fruit.State != XenomorphFruitState.Growing) { Cooldown(ent, 5); return; }
            fruit.GrowAt -= TimeSpan.FromSeconds(60);
            Cooldown(ent, fruit.GrowTime.TotalSeconds * 2 / 3);
            _popup.PopupEntity(Loc.GetString("xenomorph-surge-fruit"), ent, ent);
            return;
        }
        if (!ClearTile(coordinates) || !_plasma.ChangePlasmaAmount(ent, -75)) return;
        if (HasWeeds(coordinates))
        {
            Spawn("DumontXenomorphSurgeWall", coordinates);
            Cooldown(ent, 10);
            args.Handled = true;
            return;
        }
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 1,
            new XenomorphResinSurgeDoAfterEvent { Coordinates = GetNetCoordinates(coordinates) }, ent.Owner)
        { BreakOnMove = true, NeedHand = false, BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent });
        if (args.Handled) { Cooldown(ent, 0); SetBusy(ent, true); }
    }

    private void OnSurged(Entity<XenomorphResinSurgeComponent> ent, ref XenomorphResinSurgeDoAfterEvent args)
    {
        if (args.Handled) return;
        SetBusy(ent, false);
        if (args.Cancelled) return;
        var coordinates = GetCoordinates(args.Coordinates);
        if (!CanAct(ent) || !_interaction.InRangeUnobstructed(ent, coordinates, 7)) return;
        for (var x = -1; x <= 1; x++) for (var y = -1; y <= 1; y++)
        {
            var tile = coordinates.Offset(new Vector2(x, y));
            if (!ClearTile(tile) || !_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(coordinates), _transform.ToMapCoordinates(tile), 2)) continue;
            var blocked = false;
            foreach (var uid in _lookup.GetEntitiesIntersecting(tile))
                if (HasComp<XenomorphStickyResinComponent>(uid) || HasComp<XenomorphSurgeReinforcableComponent>(uid)) blocked = true;
            if (!blocked) Spawn("DumontXenomorphWeakStickyResin", tile);
        }
        Cooldown(ent, 10);
        args.Handled = true;
    }

    private void Absorb(Entity<XenomorphSurgeReinforcedComponent> ent, ref DamageSpecifier damage)
    {
        if (damage.GetTotal() <= 0 || ent.Comp.Remaining <= 0) return;
        damage = new DamageSpecifier(damage);
        foreach (var (type, value) in damage.DamageDict)
        {
            if (value <= 0) continue;
            var amount = FixedPoint2.Min(value, ent.Comp.Remaining);
            ent.Comp.Remaining -= amount;
            damage.DamageDict[type] -= amount;
        }
        if (ent.Comp.Remaining <= 0) RemCompDeferred<XenomorphSurgeReinforcedComponent>(ent);
    }

    private void OnDamage(Entity<XenomorphSurgeReinforcedComponent> ent, ref DamageModifyEvent args) => Absorb(ent, ref args.Damage);
    private void OnExplosion(Entity<XenomorphSurgeReinforcedComponent> ent, ref BeforeExplodeEvent args)
    {
        var damage = new DamageSpecifier(args.Damage);
        Absorb(ent, ref damage);
        foreach (var (type, value) in damage.DamageDict) args.Damage.DamageDict[type] = value;
    }

    private void OnReinforceRemoved(Entity<XenomorphSurgeReinforcedComponent> ent, ref ComponentRemove args)
    {
        if (Exists(ent.Comp.Effect)) QueueDel(ent.Comp.Effect);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphSurgeReinforcedComponent>();
        while (query.MoveNext(out var uid, out var shield))
            if (_timing.CurTime >= shield.Until) RemCompDeferred<XenomorphSurgeReinforcedComponent>(uid);
    }
}
