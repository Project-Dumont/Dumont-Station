using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphOppressorSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedStutteringSystem _stutter = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private XenomorphRageSystem _rage = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<Entity<MobStateComponent>> _nearby = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphOppressorComponent, XenomorphDislocateActionEvent>(OnDislocate);
        SubscribeLocalEvent<XenomorphOppressorComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphOppressorComponent, XenomorphAbductActionEvent>(OnAbduct);
        SubscribeLocalEvent<XenomorphOppressorComponent, XenomorphTailLashActionEvent>(OnLash);
        SubscribeLocalEvent<XenomorphOppressorComponent, XenomorphOppressorDoAfterEvent>(OnPrepared);
        SubscribeLocalEvent<XenomorphOppressorComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<XenomorphTailTetherComponent, StopThrowEvent>(OnTetherStop);
        InitializeHook();
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid) && !HasComp<XenomorphComponent>(uid);
    private bool Debuffed(EntityUid uid)
    {
        if (HasComp<StunnedComponent>(uid) || _standing.IsDown(uid)) return true;
        return _status.TryEffectsWithComp<MovementModStatusEffectComponent>(uid, out var effects)
            && effects.Any(e => e.Comp1.WalkSpeedModifier < 1 || e.Comp1.SprintSpeedModifier < 1);
    }

    private DamageSpecifier Physical(float amount, bool piercing = false)
        => new() { DamageDict = { ["Blunt"] = amount / 3, ["Slash"] = amount / 3, ["Piercing"] = amount - amount / 3 * 2 }, ArmorPenetration = piercing ? 1 : 0 };

    private void Hit(EntityUid user, EntityUid target, float amount, bool piercing = false)
    {
        var changed = _damage.TryChangeDamage(target, Physical(amount, piercing), ignoreResistances: piercing, origin: user);
        if (changed?.GetTotal() > 0) _flash.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
    }

    private void Slow(EntityUid uid, double seconds)
    {
        if (!TryComp<MovementSpeedModifierComponent>(uid, out var movement)) return;
        _slow.TryUpdateMovementSpeedModDuration(uid, "XenomorphOppressorSlowdown", TimeSpan.FromSeconds(seconds),
            1 / (1 + Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed) * 0.5f));
    }

    private void Root(EntityUid uid, double seconds)
        => _slow.TryUpdateMovementSpeedModDuration(uid, "XenomorphOppressorRoot", TimeSpan.FromSeconds(seconds), 0);

    private void Lunge(EntityUid user, EntityUid target)
    {
        var direction = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(user);
        _melee.DoLunge(user, user, user, Angle.Zero, (-_transform.GetWorldRotation(user)).RotateVec(direction), "WeaponArcThrust", Angle.Zero, false, predicted: false);
    }

    private void Throw(EntityUid user, EntityUid target, float distance)
    {
        if (Transform(target).Anchored) return;
        var direction = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(user);
        if (direction.LengthSquared() <= 0) return;
        _pulling.StopAllPulls(target);
        _throwing.TryThrow(target, Vector2.Normalize(direction) * distance, 10, user, pushbackRatio: 0, doSpin: false);
    }

    private bool Hook(EntityUid user, EntityUid target, bool seized)
    {
        if (HasComp<XenomorphTailTetherComponent>(target) || Transform(target).Anchored) return false;
        var distance = Vector2.Distance(_transform.GetWorldPosition(user), _transform.GetWorldPosition(target));
        var tether = EnsureComp<XenomorphTailTetherComponent>(target);
        tether.Source = user;
        tether.Seized = seized;
        Dirty(target, tether);
        Throw(user, target, distance < 1.3f ? -0.5f : -(distance - 1.3f));
        if (HasComp<ThrownItemComponent>(target)) return true;
        RemCompDeferred<XenomorphTailTetherComponent>(target);
        return false;
    }

    private void OnTetherStop(Entity<XenomorphTailTetherComponent> ent, ref StopThrowEvent args)
    {
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        if (ent.Comp.Seized && Victim(ent))
        {
            Slow(ent, 0.5);
            Root(ent, 1);
        }
        RemCompDeferred<XenomorphTailTetherComponent>(ent);
    }

    private void OnMelee(Entity<XenomorphOppressorComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner) return;
        foreach (var target in args.HitEntities)
        {
            if (!Victim(target) || !Debuffed(target)) continue;
            Hit(ent, target, 15);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_attack.ogg"), ent);
            break;
        }
    }

    private void OnDislocate(Entity<XenomorphOppressorComponent> ent, ref XenomorphDislocateActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Victim(args.Target) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f)
            || !_plasma.ChangePlasmaAmount(ent, -55)) return;
        _pulling.StopAllPulls(args.Target);
        var debuffed = Debuffed(args.Target);
        Hit(ent, args.Target, 20, debuffed);
        _audio.PlayPvs(new SoundCollectionSpecifier("Punch"), ent);
        if (debuffed) Root(args.Target, 1.2);
        else
        {
            Lunge(ent, args.Target);
            Throw(ent, args.Target, 1);
            Spawn("DumontXenomorphCleavePunch", new EntityCoordinates(args.Target, Vector2.Zero));
        }
        foreach (var action in _actions.GetActions(ent))
        {
            if (_actions.GetEvent(action) is not (XenomorphAbductActionEvent or XenomorphTailLashActionEvent) || action.Comp.Cooldown is not { } cooldown) continue;
            var end = cooldown.End - TimeSpan.FromSeconds(5);
            if (end <= cooldown.Start) _actions.ClearCooldown(action.AsNullable());
            else _actions.SetCooldown(action.AsNullable(), cooldown.Start, end);
        }
        args.Handled = true;
    }
}
