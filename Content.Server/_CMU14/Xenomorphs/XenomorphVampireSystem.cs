using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.Infection;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Jittering;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphVampireShovedComponent : Component;

public sealed partial class XenomorphVampireSystem : EntitySystem
{
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private SharedCameraRecoilSystem _camera = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private ThrownItemSystem _thrown = default!;
    [Dependency] private RotateToFaceSystem _rotate = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<EntityUid> _area = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphVampireComponent, XenomorphLeapHitEvent>(OnRushHit);
        SubscribeLocalEvent<XenomorphVampireComponent, XenomorphFlurryActionEvent>(OnFlurry);
        SubscribeLocalEvent<XenomorphVampireComponent, XenomorphTailJabActionEvent>(OnJab);
        SubscribeLocalEvent<XenomorphVampireComponent, XenomorphHeadbiteActionEvent>(OnHeadbite);
        SubscribeLocalEvent<XenomorphVampireComponent, XenomorphHeadbiteDoAfterEvent>(OnHeadbiteDone);
        SubscribeLocalEvent<XenomorphVampireShovedComponent, ThrowDoHitEvent>(OnSlam);
        SubscribeLocalEvent<XenomorphVampireShovedComponent, StopThrowEvent>(OnShoveStop);
    }
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid);
    private bool Burning(EntityUid uid) => TryComp<FlammableComponent>(uid, out var fire) && fire.OnFire;
    private void Roar(Entity<XenomorphVampireComponent> ent, float cooldown)
    {
        if (_timing.CurTime < ent.Comp.NextRoar) return;
        ent.Comp.NextRoar = _timing.CurTime + TimeSpan.FromSeconds(cooldown);
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
    }
    private void Hit(EntityUid user, EntityUid target, float blunt, float slash, float piercing, string effect, bool tailBonus = false)
    {
        var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = blunt, ["Slash"] = slash, ["Piercing"] = piercing } };
        if (tailBonus)
        {
            var bonus = new XenomorphTailDamageEvent(new DamageSpecifier());
            RaiseLocalEvent(user, ref bonus);
            damage += bonus.Damage;
        }
        _damage.TryChangeDamage(target, damage, origin: user);
        if (TerminatingOrDeleted(target)) return;
        Spawn(effect, new EntityCoordinates(target, Vector2.Zero));
        _flash.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
    }
    private void OnRushHit(Entity<XenomorphVampireComponent> ent, ref XenomorphLeapHitEvent args)
    {
        if (!Victim(args.Target)) return;
        Hit(ent, args.Target, 13.33f, 13.33f, 13.34f, "DumontXenomorphVampireSlash");
        _jitter.DoJitter(args.Target, TimeSpan.FromSeconds(1), false);
        var target = args.Target;
        _camera.KickCamera(target, _random.NextVector2Box(-1, 1));
        Timer.Spawn(TimeSpan.FromSeconds(.1), () => { if (Exists(target)) _camera.KickCamera(target, _random.NextVector2Box(-1, 1)); });
    }
    private void OnFlurry(Entity<XenomorphVampireComponent> ent, ref XenomorphFlurryActionEvent args)
    {
        if (args.Handled || !Ready(ent) || Transform(ent).GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var delta = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(delta.LengthSquared()) || delta.LengthSquared() < .001f) return;
        var area = Box2.CenteredAround(origin.Position, new Vector2(1, 2)).Translated(new Vector2(0, 1.5f));
        var rotated = new Box2Rotated(area, delta.ToAngle() - Angle.FromDegrees(90), origin.Position);
        _area.Clear();
        _lookup.GetEntitiesIntersecting(gridUid, rotated, _area, LookupFlags.Dynamic | LookupFlags.Static);
        var hits = 0;
        foreach (var victim in _area.Where(Victim).OrderBy(uid => (_transform.GetWorldPosition(uid) - origin.Position).LengthSquared()))
        {
            if (!_interaction.InRangeUnobstructed(ent.Owner, victim, range: 2.5f)) continue;
            Hit(ent, victim, 11.66f, 11.67f, 11.67f, "DumontXenomorphVampireSlash", true);
            if (hits == 0) Lunge(ent, victim);
            _audio.PlayPvs(new SoundCollectionSpecifier("AlienClaw"), victim);
            Spawn("DumontXenomorphVampireFlurryHeal", new EntityCoordinates(ent, Vector2.Zero));
            if (!Burning(ent))
            {
                ent.Comp.FlurryHeal += 30;
                ent.Comp.FlurryHealAt = _timing.CurTime + TimeSpan.FromSeconds(1);
            }
            if (++hits >= 4) break;
        }
        foreach (var tile in _map.GetTilesIntersecting(gridUid, grid, rotated))
        {
            var point = _turf.GetTileCenter(tile);
            if (_interaction.InRangeUnobstructed(ent.Owner, point, range: 2.5f)) Spawn("DumontXenomorphVampireFlurryTelegraph", point);
        }
        Roar(ent, 6);
        args.Handled = true;
    }
    private void OnJab(Entity<XenomorphVampireComponent> ent, ref XenomorphTailJabActionEvent args)
    {
        if (args.Handled || !Ready(ent) || (!Victim(args.Target) && !(HasComp<DamageableComponent>(args.Target) && !HasComp<MobStateComponent>(args.Target))) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 2.5f)) return;
        Hit(ent, args.Target, 16.66f, 16.67f, 16.67f, "DumontXenomorphTailHit", true);
        if (TryComp<XenomorphRavagerObstacleComponent>(args.Target, out var obstacle) && obstacle.Scissor)
            _damage.TryChangeDamage(args.Target, new DamageSpecifier { DamageDict = { ["Structural"] = 100 } }, origin: ent);
        if (!TerminatingOrDeleted(args.Target)) Lunge(ent, args.Target);
        if (TerminatingOrDeleted(args.Target)) { args.Handled = true; return; }
        if (HasComp<MobStateComponent>(args.Target)) _slow.TryUpdateMovementSpeedModDuration(args.Target, "XenomorphVampireSlowdown", TimeSpan.FromSeconds(.5), .5f);
        var direction = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        if (direction.LengthSquared() > .001f && TryComp<PhysicsComponent>(args.Target, out var body) && !Transform(args.Target).Anchored)
        {
            EnsureComp<XenomorphVampireShovedComponent>(args.Target);
            _physics.SetLinearVelocity(args.Target, Vector2.Zero, body: body);
            _throwing.TryThrow(args.Target, Vector2.Normalize(direction), 5, ent, pushbackRatio: 0, doSpin: false);
            if (!HasComp<ThrownItemComponent>(args.Target)) RemCompDeferred<XenomorphVampireShovedComponent>(args.Target);
        }
        _rotate.TryFaceAngle(ent, _transform.GetWorldRotation(ent) - Angle.FromDegrees(180));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_tail_attack.ogg"), ent);
        Roar(ent, 5);
        args.Handled = true;
    }
    private void Lunge(EntityUid user, EntityUid target)
    {
        var offset = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(user);
        if (offset.LengthSquared() < .001f) return;
        var local = (-_transform.GetWorldRotation(user)).RotateVec(Vector2.Normalize(offset));
        _melee.DoLunge(user, user, user, Angle.Zero, local, "WeaponArcThrust", Angle.Zero, false, predicted: false);
    }
    private void OnSlam(Entity<XenomorphVampireShovedComponent> ent, ref ThrowDoHitEvent args)
    {
        if (args.Handled || HasComp<MobStateComponent>(args.Target) || !TryComp<PhysicsComponent>(args.Target, out var wall) || !wall.Hard
            || !TryComp<PhysicsComponent>(ent, out var body)) return;
        var speed = body.LinearVelocity.Length();
        args.Handled = true;
        _damage.TryChangeDamage(ent, new DamageSpecifier { DamageDict = { ["Blunt"] = (HasComp<HumanoidAppearanceComponent>(ent) ? 11 : 21) * .2f * speed } });
        _stun.TryUpdateParalyzeDuration(ent, TimeSpan.FromSeconds(.5));
        _slow.TryUpdateMovementSpeedModDuration(ent, "XenomorphVampireSlowdown", TimeSpan.FromSeconds(.5), .5f);
        _thrown.StopThrow(ent, args.Component);
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        _audio.PlayPvs(new SoundCollectionSpecifier("MetalSlam"), args.Target);
        _flash.RaiseEffect(Color.Red, new List<EntityUid> { ent }, Filter.Pvs(ent.Owner, entityManager: EntityManager));
        RemCompDeferred<XenomorphVampireShovedComponent>(ent);
    }
    private void OnShoveStop(Entity<XenomorphVampireShovedComponent> ent, ref StopThrowEvent args)
    {
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        RemCompDeferred<XenomorphVampireShovedComponent>(ent);
    }
    private bool CanHeadbite(EntityUid user, EntityUid target)
        => Ready(user) && Victim(target) && !_mobs.IsDead(target) && !HasComp<XenomorphInfectionComponent>(target)
           && (_mobs.IsCritical(target) || _status.HasStatusEffect(target, "StatusEffectForcedSleeping"))
           && _interaction.InRangeUnobstructed(user, target, range: 1.5f);
    private void OnHeadbite(Entity<XenomorphVampireComponent> ent, ref XenomorphHeadbiteActionEvent args)
    {
        if (args.Handled || ent.Comp.Headbiting) return;
        if (!CanHeadbite(ent, args.Target))
        {
            _popup.PopupEntity(Loc.GetString(HasComp<XenomorphInfectionComponent>(args.Target) ? "dumont-xenomorph-headbite-infected" : "dumont-xenomorph-headbite-invalid"), ent, ent);
            return;
        }
        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, .8f, new XenomorphHeadbiteDoAfterEvent(), ent, args.Target)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = false, DistanceThreshold = 1.5f })) return;
        ent.Comp.Headbiting = true;
        ent.Comp.HeadbiteAction = args.Action.Owner;
        Dirty(ent);
        _actions.SetToggled(args.Action.Owner, true);
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-headbite-start"), ent, ent);
        args.Handled = true;
    }
    private void OnHeadbiteDone(Entity<XenomorphVampireComponent> ent, ref XenomorphHeadbiteDoAfterEvent args)
    {
        if (args.Handled) return;
        ent.Comp.Headbiting = false;
        Dirty(ent);
        if (ent.Comp.HeadbiteAction is { } action) _actions.SetToggled(action, false);
        ent.Comp.HeadbiteAction = null;
        if (args.Cancelled || args.Target is not { } target || !CanHeadbite(ent, target)) return;
        args.Handled = true;
        Hit(ent, target, 20, 20, 20, "DumontXenomorphVampireHeadbite");
        if (!TerminatingOrDeleted(target) && !_mobs.IsDead(target) && _thresholds.TryGetDeadThreshold(target, out var threshold) && TryComp<DamageableComponent>(target, out var damage))
        {
            var amount = Math.Max(0, threshold.Value.Float() - damage.TotalDamage.Float()) + .1f;
            _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Asphyxiation"] = amount } }, ignoreResistances: true, origin: ent,
                targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
        }
        if (!Burning(ent)) { ent.Comp.BiteHeal += 150; ent.Comp.BiteHealAt = _timing.CurTime + TimeSpan.FromSeconds(.05); }
        Spawn("DumontXenomorphVampireBiteHeal", new EntityCoordinates(ent, Vector2.Zero));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_bite2.ogg"), ent);
        _jitter.DoJitter(ent, TimeSpan.FromSeconds(3), true, 80, 8, true);
        Roar(ent, 5);
    }
    private void Heal(EntityUid uid, int amount)
    {
        if (_mobs.IsDead(uid) || !TryComp<DamageableComponent>(uid, out var damage)) return;
        var heal = new DamageSpecifier();
        float remaining = amount;
        foreach (var type in new[] { "Blunt", "Slash", "Piercing", "Heat", "Shock", "Cold", "Caustic" })
        {
            if (remaining <= 0 || !damage.Damage.DamageDict.TryGetValue(type, out var value) || value <= 0) continue;
            var restore = Math.Min(value.Float(), remaining);
            heal.DamageDict[type] = -restore;
            remaining -= restore;
        }
        _damage.TryChangeDamage(uid, heal, ignoreResistances: true, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
    }
    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphVampireComponent>();
        while (query.MoveNext(out var uid, out var vampire))
        {
            if (vampire.FlurryHeal > 0 && now >= vampire.FlurryHealAt) { Heal(uid, vampire.FlurryHeal); vampire.FlurryHeal = 0; }
            if (vampire.BiteHeal > 0 && now >= vampire.BiteHealAt) { Heal(uid, vampire.BiteHeal); vampire.BiteHeal = 0; }
        }
    }
}
