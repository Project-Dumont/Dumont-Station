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
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphVanguardSystem : EntitySystem
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
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private PullingSystem _pulling = default!;
    private readonly HashSet<Entity<MobStateComponent>> _nearby = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVanguardComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphVanguardComponent, DamageModifyEvent>(OnDamage, after: new[] { typeof(XenomorphArmorSystem) });
        SubscribeLocalEvent<XenomorphVanguardComponent, XenomorphPierceActionEvent>(OnPierce);
        SubscribeLocalEvent<XenomorphVanguardComponent, XenomorphCleaveActionEvent>(OnCleave);
        SubscribeLocalEvent<XenomorphVanguardComponent, XenomorphLeapActionEvent>(OnBlitz, before: new[] { typeof(XenomorphLeapSystem) });
        SubscribeLocalEvent<XenomorphVanguardComponent, XenomorphLeapStartedEvent>(OnLeapStarted);
    }

    private void OnInit(Entity<XenomorphVanguardComponent> ent, ref MapInitEvent args) => Regen(ent);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid) && !HasComp<XenomorphComponent>(uid);
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Buffed(Entity<XenomorphVanguardComponent> ent) => ent.Comp.Shield > 0 || ent.Comp.NextDecay + TimeSpan.FromSeconds(0.7) > _timing.CurTime;

    public void Regen(Entity<XenomorphVanguardComponent> ent)
    {
        if (TryComp<XenomorphFruitBuffComponent>(ent, out var fruit) && fruit.Shield > 0)
        {
            fruit.Shield = 0;
            if (Exists(fruit.ShieldEffect)) QueueDel(fruit.ShieldEffect);
        }
        if (ent.Comp.Shield <= 0) _popup.PopupEntity(Loc.GetString("dumont-xenomorph-vanguard-shield-regen"), ent, ent);
        ent.Comp.Shield = 800;
        ent.Comp.ShieldHit = false;
        ent.Comp.LastRecharge = _timing.CurTime;
        Dirty(ent);
    }

    public void RemoveShield(Entity<XenomorphVanguardComponent> ent)
    {
        ent.Comp.Shield = 0;
        ent.Comp.NextDecay = _timing.CurTime;
        Dirty(ent);
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-vanguard-shield-break"), ent, ent);
    }

    private void OnDamage(Entity<XenomorphVanguardComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Shield <= 0 || args.Damage.GetTotal() <= 0) return;
        ent.Comp.LastHit = _timing.CurTime;
        args.Damage = new DamageSpecifier(args.Damage);
        if (!ent.Comp.ShieldHit && args.Damage.GetTotal() > 5)
        {
            ent.Comp.ShieldHit = true;
            foreach (var type in args.Damage.DamageDict.Keys) if (args.Damage.DamageDict[type] > 0) args.Damage.DamageDict[type] = 0;
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-vanguard-shield-hit"), ent, ent);
            return;
        }
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0) continue;
            var absorbed = Content.Goobstation.Maths.FixedPoint.FixedPoint2.New(Math.Min(value.Float(), ent.Comp.Shield));
            ent.Comp.Shield -= absorbed.Float();
            args.Damage.DamageDict[type] -= absorbed;
        }
        Dirty(ent);
        if (ent.Comp.Shield <= 0)
        {
            RemoveShield(ent);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Bullets/shield_break_c1.ogg"), ent);
        }
    }

    private void Lunge(EntityUid uid, Vector2 direction)
        => _melee.DoLunge(uid, uid, uid, Angle.Zero, (-_transform.GetWorldRotation(uid)).RotateVec(direction), "WeaponArcThrust", Angle.Zero, false, predicted: false);

    private void Hit(EntityUid uid, EntityUid target, float amount, float penetration = 0)
    {
        var part = amount / 3;
        var changed = _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Blunt"] = part, ["Slash"] = part, ["Piercing"] = amount - part * 2 }, ArmorPenetration = penetration }, origin: uid);
        if (changed?.GetTotal() > 0) _flash.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
        Spawn("DumontXenomorphExtraSlash", new EntityCoordinates(target, Vector2.Zero));
    }

    private void OnPierce(Entity<XenomorphVanguardComponent> ent, ref XenomorphPierceActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < 0.0001f
            || Transform(ent).GridUid == null || !_plasma.ChangePlasmaAmount(ent, -50)) return;
        direction = Vector2.Normalize(direction);
        _nearby.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 3.6f, _nearby);
        var hits = 0;
        foreach (var mob in _nearby.OrderBy(m => Vector2.DistanceSquared(origin.Position, _transform.GetWorldPosition(m))))
        {
            if (!Victim(mob)) continue;
            var offset = _transform.GetWorldPosition(mob) - origin.Position;
            var along = Vector2.Dot(offset, direction);
            var across = Math.Abs(offset.X * direction.Y - offset.Y * direction.X);
            if (along < 0 || along > 3 || across > 0.6f || !_interaction.InRangeUnobstructed(ent.Owner, mob.Owner, range: 3.6f)) continue;
            Hit(ent, mob, 45, 0.2f);
            if (++hits >= 4) break;
        }
        if (hits >= 2) Regen(ent);
        Lunge(ent, direction * 3);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_attack.ogg"), ent);
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
        args.Handled = true;
    }

    private void OnCleave(Entity<XenomorphVanguardComponent> ent, ref XenomorphCleaveActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Victim(args.Target) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f)) return;
        var buffed = Buffed(ent);
        var direction = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        Lunge(ent, direction);
        _audio.PlayPvs(new SoundCollectionSpecifier("Punch"), ent);
        if (args.Flings)
        {
            _pulling.StopAllPulls(args.Target);
            if (!Transform(args.Target).Anchored && direction.LengthSquared() > 0)
                _throwing.TryThrow(args.Target, Vector2.Normalize(direction) * (buffed ? 6 : 3), 10, ent, pushbackRatio: 0, doSpin: false);
            Spawn("DumontXenomorphTailSlam", new EntityCoordinates(args.Target, Vector2.Zero));
        }
        else
        {
            _slow.TryUpdateMovementSpeedModDuration(args.Target, "XenomorphVanguardRoot", TimeSpan.FromSeconds(buffed ? 1.8 : 1), 0);
            Spawn("DumontXenomorphCleavePunch", new EntityCoordinates(args.Target, Vector2.Zero));
        }
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphCleaveActionEvent) _actions.SetCooldown(action.Owner, TimeSpan.FromSeconds(12));
        args.Handled = true;
    }

    private void OnBlitz(Entity<XenomorphVanguardComponent> ent, ref XenomorphLeapActionEvent args)
    {
        if (args.Handled || !ent.Comp.BlitzActive) return;
        args.Handled = true;
        if (!Comp<XenomorphLeapComponent>(ent).Leaping && ent.Comp.SlashReady) Slash(ent);
    }

    private void OnLeapStarted(Entity<XenomorphVanguardComponent> ent, ref XenomorphLeapStartedEvent args)
    {
        ent.Comp.BlitzActive = true;
        ent.Comp.SlashReady = false;
        ent.Comp.BlitzStarted = _timing.CurTime;
        ToggleBlitz(ent, true);
        Dirty(ent);
    }

    private void ToggleBlitz(EntityUid uid, bool active)
    {
        foreach (var action in _actions.GetActions(uid))
        {
            if (_actions.GetEvent(action) is not XenomorphLeapActionEvent) continue;
            _actions.SetToggled(action.Owner, active);
            var delay = active ? TimeSpan.Zero : TimeSpan.FromSeconds(11) - (_timing.CurTime - Comp<XenomorphVanguardComponent>(uid).BlitzStarted);
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
            _actions.SetUseDelay(action.Owner, delay);
            _actions.SetCooldown(action.Owner, delay);
        }
    }

    private void Slash(Entity<XenomorphVanguardComponent> ent)
    {
        ent.Comp.BlitzActive = false;
        ent.Comp.SlashReady = false;
        ToggleBlitz(ent, false);
        Dirty(ent);
        if (!Ready(ent)) return;
        EnsureComp<XenomorphSweepingComponent>(ent);
        _nearby.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 1.5f, _nearby);
        var hits = 0;
        foreach (var mob in _nearby)
        {
            if (!Victim(mob) || !_interaction.InRangeUnobstructed(ent.Owner, mob.Owner, range: 1.5f)) continue;
            Hit(ent, mob, 40);
            hits++;
        }
        if (hits > 0)
        {
            _audio.PlayPvs(new SoundCollectionSpecifier("AlienClaw"), ent);
            Regen(ent);
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphVanguardComponent>();
        while (query.MoveNext(out var uid, out var vanguard))
        {
            if (vanguard.LastRecharge <= vanguard.LastHit && vanguard.LastHit + TimeSpan.FromSeconds(20) <= _timing.CurTime) Regen((uid, vanguard));
            if (vanguard.Shield > 0 && vanguard.ShieldHit && vanguard.NextDecay <= _timing.CurTime)
            {
                vanguard.NextDecay = _timing.CurTime + TimeSpan.FromSeconds(0.4);
                vanguard.Shield = Math.Max(0, vanguard.Shield * 0.7f - 50);
                Dirty(uid, vanguard);
                if (vanguard.Shield <= 0) RemoveShield((uid, vanguard));
            }
            if (!vanguard.BlitzActive) continue;
            if (!vanguard.SlashReady && !Comp<XenomorphLeapComponent>(uid).Leaping)
            {
                vanguard.SlashReady = true;
                vanguard.SlashAt = _timing.CurTime + TimeSpan.FromSeconds(3);
            }
            if (vanguard.SlashReady && vanguard.SlashAt <= _timing.CurTime) Slash((uid, vanguard));
        }
    }
}
