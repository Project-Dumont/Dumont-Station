using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphBombardProjectileComponent : Component
{
    [DataField] public bool Neurotoxin;
    public EntityUid? Shooter;
    public MapCoordinates Origin;
    public float Range = 10;
}

public sealed partial class XenomorphBoilerSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private XenomorphSpitterSystem _spitter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBoilerComponent, XenomorphBombardActionEvent>(OnBombard);
        SubscribeLocalEvent<XenomorphBoilerComponent, XenomorphBombardDoAfterEvent>(OnBombarded);
        SubscribeLocalEvent<XenomorphBoilerComponent, XenomorphAcidShroudActionEvent>(OnShroud);
        SubscribeLocalEvent<XenomorphBoilerComponent, XenomorphAcidShroudDoAfterEvent>(OnShrouded);
        SubscribeLocalEvent<XenomorphBoilerComponent, XenomorphToggleGasActionEvent>(OnToggle);
        SubscribeLocalEvent<XenomorphBoilerComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<XenomorphBombardProjectileComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphBombardProjectileComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphBombardProjectileComponent, EntityTerminatingEvent>(OnLanded);
        InitializeGas();
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid)
        && !_containers.IsEntityInContainer(uid);

    private void OnToggle(Entity<XenomorphBoilerComponent> ent, ref XenomorphToggleGasActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        ent.Comp.Neurotoxin = !ent.Comp.Neurotoxin;
        Dirty(ent);
        _actions.SetToggled(args.Action.Owner, ent.Comp.Neurotoxin);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphBombardActionEvent && TryComp<XenomorphActionAreaComponent>(action, out var area))
            {
                area.Range = ent.Comp.Neurotoxin ? 4 : 3;
                Dirty(action.Owner, area);
            }
        _popup.PopupEntity(Loc.GetString(ent.Comp.Neurotoxin ? "xenomorph-gas-neuro" : "xenomorph-gas-acid"), ent, ent);
        args.Handled = true;
    }

    private void Preparing(Entity<XenomorphBoilerComponent> ent, EntityUid? action)
    {
        ent.Comp.Preparing = action != null;
        ent.Comp.PreparingAction = action;
        Dirty(ent);
        foreach (var owned in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(owned) is not (XenomorphBombardActionEvent or XenomorphAcidShroudActionEvent or XenomorphSprayAcidActionEvent)) continue;
            _actions.SetEnabled(owned.Owner, action == null);
            _actions.SetToggled(owned.Owner, owned.Owner == action);
        }
    }

    private void Cooldown(EntityUid uid, bool shroud)
    {
        var reduction = TryComp<XenomorphFruitBuffComponent>(uid, out var fruit) && _timing.CurTime < fruit.SporeUntil ? fruit.SporeReduction : 0;
        foreach (var action in _actions.GetActions(uid))
        {
            var ev = _actions.GetEvent(action);
            if (ev is not (XenomorphBombardActionEvent or XenomorphAcidShroudActionEvent) && !(shroud && ev is XenomorphSprayAcidActionEvent)) continue;
            _actions.SetCooldown(action.AsNullable(), _timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(30 * (1 - reduction)));
        }
    }

    private void OnBombard(Entity<XenomorphBoilerComponent> ent, ref XenomorphBombardActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !Ready(ent) || !_plasma.HasPlasma(ent, 200)) return;
        var source = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        if (source.MapId != target.MapId || !float.IsFinite(target.Position.LengthSquared())) return;
        var delta = target.Position - source.Position;
        if (delta.LengthSquared() < 0.01f) return;
        if (delta.LengthSquared() > 100) target = source.Offset(Vector2.Normalize(delta) * 10);
        var coordinates = _transform.ToCoordinates(Transform(ent).GridUid ?? Transform(ent).MapUid!.Value, target);
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(4.5),
            new XenomorphBombardDoAfterEvent { Coordinates = GetNetCoordinates(coordinates) }, ent)
            { BreakOnMove = true, NeedHand = false });
        if (!args.Handled) return;
        Preparing(ent, args.Action.Owner);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_drool1.ogg"), ent);
        _popup.PopupEntity(Loc.GetString("xenomorph-bombard-prepare"), ent, ent);
    }

    private void OnBombarded(Entity<XenomorphBoilerComponent> ent, ref XenomorphBombardDoAfterEvent args)
    {
        Preparing(ent, null);
        if (args.Cancelled || args.Handled || !Ready(ent) || !_plasma.ChangePlasmaAmount(ent, -200)) return;
        var source = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(GetCoordinates(args.Coordinates));
        if (target.MapId != source.MapId) return;
        var direction = target.Position - source.Position;
        if (direction.LengthSquared() < 0.01f) return;
        var projectile = Spawn(ent.Comp.Neurotoxin ? "DumontXenomorphBombardNeuro" : "DumontXenomorphBombardAcid", _transform.GetMoverCoordinates(ent));
        var comp = Comp<XenomorphBombardProjectileComponent>(projectile);
        comp.Shooter = ent;
        comp.Origin = source;
        comp.Range = Math.Min(10, direction.Length());
        _guns.ShootProjectile(projectile, direction, Vector2.Zero, ent, ent, 7.5f);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/blobattack.ogg"), ent);
        Cooldown(ent, false);
        args.Handled = true;
    }

    private void OnShroud(Entity<XenomorphBoilerComponent> ent, ref XenomorphAcidShroudActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !Ready(ent)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(0.75),
            new XenomorphAcidShroudDoAfterEvent(), ent) { BreakOnMove = true, NeedHand = false });
        if (args.Handled) Preparing(ent, args.Action.Owner);
    }

    private void OnShrouded(Entity<XenomorphBoilerComponent> ent, ref XenomorphAcidShroudDoAfterEvent args)
    {
        Preparing(ent, null);
        if (args.Cancelled || args.Handled || !Ready(ent)) return;
        CreateCloud(_transform.GetMoverCoordinates(ent), ent.Comp.Neurotoxin, 1, ent);
        Cooldown(ent, true);
        args.Handled = true;
    }

    private void OnState(Entity<XenomorphBoilerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive && ent.Comp.Preparing) Preparing(ent, null);
        if (args.NewMobState == MobState.Dead) CreateCloud(_transform.GetMoverCoordinates(ent), false, 2, ent);
    }

    private void OnCollide(Entity<XenomorphBombardProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity)) args.Cancelled = true;
    }

    private void OnHit(Entity<XenomorphBombardProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!Victim(args.Target)) return;
        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(4));
        if (ent.Comp.Neurotoxin) InjectNeuro(args.Target, 5, false);
        else
        {
            _spitter.ApplyAcid(args.Target);
            if (TryComp<XenomorphAcidedComponent>(args.Target, out var acid)) acid.DamagePerTick = 1;
        }
    }

    private void OnLanded(Entity<XenomorphBombardProjectileComponent> ent, ref EntityTerminatingEvent args)
    {
        var coordinates = _transform.GetMoverCoordinates(ent);
        if (_transform.ToMapCoordinates(coordinates).MapId == MapId.Nullspace) return;
        var position = _transform.ToMapCoordinates(coordinates);
        var delta = position.Position - ent.Comp.Origin.Position;
        if (position.MapId == ent.Comp.Origin.MapId && delta.LengthSquared() > 0.01f)
            coordinates = _transform.ToCoordinates(coordinates.EntityId, position.Offset(-Vector2.Normalize(delta) * 0.5f));
        CreateCloud(coordinates, ent.Comp.Neurotoxin, ent.Comp.Neurotoxin ? 4 : 3, ent.Comp.Shooter);
    }

    private void UpdateProjectiles()
    {
        var query = EntityQueryEnumerator<XenomorphBombardProjectileComponent>();
        while (query.MoveNext(out var uid, out var projectile))
        {
            var position = _transform.GetMapCoordinates(uid);
            if (projectile.Origin.MapId != position.MapId || (position.Position - projectile.Origin.Position).LengthSquared() >= projectile.Range * projectile.Range) QueueDel(uid);
        }
    }

    public override void Update(float frameTime)
    {
        UpdateProjectiles();
        UpdateGas();
    }
}
