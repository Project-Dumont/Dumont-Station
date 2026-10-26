using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphSecondImpaleComponent : Component
{
    public EntityUid Origin;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan At;
}

public sealed partial class XenomorphDancerSystem
{
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedStutteringSystem _stutter = default!;
    [Dependency] private XenomorphRageSystem _rage = default!;
    private readonly List<EntityUid> _yellowTargets = new();

    private void InitializeCombat()
    {
        SubscribeLocalEvent<XenomorphDancerComponent, XenomorphImpaleActionEvent>(OnImpale);
        SubscribeLocalEvent<XenomorphDancerComponent, XenomorphTailTripActionEvent>(OnTrip);
        SubscribeLocalEvent<XenomorphDancerComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphDancerComponent, ProjectileReflectAttemptEvent>(OnProjectile);
        SubscribeLocalEvent<XenomorphDancerComponent, XenomorphRestActionEvent>(OnRest, after: new[] { typeof(XenomorphRestSystem) });
    }

    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid) && !HasComp<XenomorphComponent>(uid);
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Red(EntityUid uid) => TryComp<XenomorphDancerMarkComponent>(uid, out var mark) && mark.RedUntil > _timing.CurTime;
    private bool Yellow(EntityUid uid) => TryComp<XenomorphDancerMarkComponent>(uid, out var mark) && mark.YellowUntil > _timing.CurTime;

    private void OnProjectile(Entity<XenomorphDancerComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || args.Component.Shooter is { } shooter && HasComp<XenomorphComponent>(shooter)
            || HasComp<XenomorphSpitProjectileComponent>(args.ProjUid)) return;
        ent.Comp.ProjectileHits++;
        if (ent.Comp.ProjectileHits < (ent.Comp.Dodging ? 3 : 6)) return;
        ent.Comp.ProjectileHits = 0;
        args.Cancelled = true;
        QueueDel(args.ProjUid);
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-dancer-projectile-dodge"), ent, ent);
    }

    private void OnRest(Entity<XenomorphDancerComponent> ent, ref XenomorphRestActionEvent args)
    {
        if (TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting) ent.Comp.ProjectileHits = 0;
    }

    private void ResetTails(EntityUid uid)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphImpaleActionEvent or XenomorphTailTripActionEvent)
                _actions.SetCooldown(action.Owner, TimeSpan.Zero);
    }

    private void ConsumeYellow(EntityUid uid, EntityUid target, EntityUid action)
    {
        if (!Yellow(target)) return;
        var mark = Comp<XenomorphDancerMarkComponent>(target);
        mark.YellowUntil = TimeSpan.Zero;
        Dirty(target, mark);
        ResetTails(uid);
        _actions.SetUseDelay(action, TimeSpan.Zero);
    }

    private void OnMelee(Entity<XenomorphDancerComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit) return;
        foreach (var target in args.HitEntities)
        {
            if (!Victim(target)) continue;
            if (Yellow(target)) ResetTails(ent);
            if (_mobs.IsCritical(target) && _timing.CurTime >= ent.Comp.NextYellowSpread) Spread(ent, target);
            var mark = EnsureComp<XenomorphDancerMarkComponent>(target);
            mark.RedUntil = _timing.CurTime + TimeSpan.FromSeconds(6);
            Dirty(target, mark);
            break;
        }
    }

    private void Spread(Entity<XenomorphDancerComponent> ent, EntityUid center)
    {
        ent.Comp.NextYellowSpread = _timing.CurTime + TimeSpan.FromSeconds(20);
        _nearby.Clear();
        _yellowTargets.Clear();
        _lookup.GetEntitiesInRange(Transform(center).Coordinates, 5, _nearby);
        foreach (var mob in _nearby)
            if (mob.Owner != center && Victim(mob) && _mobs.IsAlive(mob) && !Yellow(mob)) _yellowTargets.Add(mob);
        var position = _transform.GetWorldPosition(center);
        _yellowTargets.Sort((a,b) =>
        {
            var distance = Vector2.DistanceSquared(_transform.GetWorldPosition(a), position).CompareTo(Vector2.DistanceSquared(_transform.GetWorldPosition(b), position));
            return distance != 0 ? distance : a.Id.CompareTo(b.Id);
        });
        for (var i = 0; i < Math.Min(5, _yellowTargets.Count); i++)
        {
            var target = _yellowTargets[i];
            var mark = EnsureComp<XenomorphDancerMarkComponent>(target);
            mark.YellowUntil = _timing.CurTime + TimeSpan.FromSeconds(10);
            Dirty(target, mark);
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-dancer-yellow-target"), target, target);
        }
    }

    private void Lunge(EntityUid uid, EntityCoordinates coordinates)
    {
        var origin = _transform.GetMapCoordinates(uid);
        var target = _transform.ToMapCoordinates(coordinates);
        if (origin.MapId != target.MapId) return;
        var direction = target.Position - origin.Position;
        _melee.DoLunge(uid, uid, uid, Angle.Zero, (-_transform.GetWorldRotation(uid)).RotateVec(direction), "WeaponArcThrust", Angle.Zero, false, predicted: false);
    }

    private void Impale(EntityUid uid, EntityUid target)
    {
        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Blunt"] = 20, ["Slash"] = 20, ["Piercing"] = 20 }, ArmorPenetration = 0.3f }, origin: uid);
        Spawn("DumontXenomorphTailHit", new EntityCoordinates(target, Vector2.Zero));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_attack.ogg"), uid);
        Lunge(uid, Transform(target).Coordinates);
    }

    private void OnImpale(Entity<XenomorphDancerComponent> ent, ref XenomorphImpaleActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        _actions.SetUseDelay(args.Action.Owner, TimeSpan.FromSeconds(13));
        var selected = args.Entity;
        if (selected == null)
        {
            _nearby.Clear();
            _lookup.GetEntitiesInRange(args.Target, .5f, _nearby);
            foreach (var mob in _nearby)
                if (Victim(mob)) { selected = mob.Owner; break; }
        }
        if (selected is not { } target || !Victim(target) || !_interaction.InRangeUnobstructed(ent.Owner, target, range: 2))
        {
            _actions.SetCooldown(args.Action.Owner, TimeSpan.FromSeconds(1));
            Lunge(ent, args.Target);
            Spawn("DumontXenomorphTailHit", args.Target);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_attack.ogg"), ent);
            return;
        }
        if (!_plasma.ChangePlasmaAmount(ent, -80)) return;
        if (Red(target))
        {
            var second = EnsureComp<XenomorphSecondImpaleComponent>(target);
            second.Origin = ent;
            second.At = _timing.CurTime + TimeSpan.FromSeconds(0.4);
            var mark = Comp<XenomorphDancerMarkComponent>(target);
            mark.RedUntil = TimeSpan.Zero;
            Dirty(target, mark);
            _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
        }
        Impale(ent, target);
        ConsumeYellow(ent, target, args.Action.Owner);
        args.Handled = true;
    }

    private void OnTrip(Entity<XenomorphDancerComponent> ent, ref XenomorphTailTripActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Victim(args.Target) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 2)
            || !_plasma.ChangePlasmaAmount(ent, -30)) return;
        _actions.SetUseDelay(args.Action.Owner, TimeSpan.FromSeconds(13));
        Lunge(ent, Transform(args.Target).Coordinates);
        Spawn("DumontXenomorphDisarm", new EntityCoordinates(args.Target, Vector2.Zero));
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoTailSwipe"), ent);
        var red = Red(args.Target);
        _stun.TryUpdateParalyzeDuration(args.Target, TimeSpan.FromSeconds(red ? 2 : 0.2));
        if (red)
        {
            _rage.Daze(args.Target);
            _stutter.DoStutter(args.Target, TimeSpan.FromSeconds(4), true);
            var mark = Comp<XenomorphDancerMarkComponent>(args.Target);
            mark.RedUntil = TimeSpan.Zero;
            Dirty(args.Target, mark);
        }
        else if (TryComp<MovementSpeedModifierComponent>(args.Target, out var movement))
            _slow.TryUpdateMovementSpeedModDuration(args.Target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(0.3), 1 / (1 + movement.CurrentSprintSpeed * 0.5f));
        ConsumeYellow(ent, args.Target, args.Action.Owner);
        args.Handled = true;
    }

    private void UpdateCombat()
    {
        var seconds = EntityQueryEnumerator<XenomorphSecondImpaleComponent>();
        while (seconds.MoveNext(out var uid, out var hit))
        {
            if (_timing.CurTime < hit.At) continue;
            if (!TerminatingOrDeleted(hit.Origin) && Victim(uid)) Impale(hit.Origin, uid);
            RemCompDeferred<XenomorphSecondImpaleComponent>(uid);
        }
        var marks = EntityQueryEnumerator<XenomorphDancerMarkComponent>();
        while (marks.MoveNext(out var uid, out var mark))
            if (mark.RedUntil <= _timing.CurTime && mark.YellowUntil <= _timing.CurTime) RemCompDeferred<XenomorphDancerMarkComponent>(uid);
    }
}
