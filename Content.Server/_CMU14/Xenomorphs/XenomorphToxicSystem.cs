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
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphToxicProjectileComponent : Component;

public sealed partial class XenomorphToxicSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private XenomorphAciderSystem _corrosion = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphToxicComponent, XenomorphToxicSpitActionEvent>(OnSpit);
        SubscribeLocalEvent<XenomorphToxicComponent, XenomorphToxicSlashActionEvent>(OnSlash);
        SubscribeLocalEvent<XenomorphToxicComponent, XenomorphDrainStingActionEvent>(OnSting);
        SubscribeLocalEvent<XenomorphToxicComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphToxicProjectileComponent, ProjectileHitEvent>(OnSpitHit);
        SubscribeLocalEvent<XenomorphToxicComponent, XenomorphWeakCorrodeActionEvent>(OnCorrode);
        SubscribeLocalEvent<XenomorphToxicComponent, XenomorphWeakCorrodeDoAfterEvent>(OnCorrodeDone);
        InitializeIntoxication();
    }
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid);
    private void OnSpit(Entity<XenomorphToxicComponent> ent, ref XenomorphToxicSpitActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 30)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < .001f) return;
        var projectile = Spawn("DumontXenomorphToxicSpit", _transform.GetMoverCoordinates(ent));
        Comp<XenomorphSpitProjectileComponent>(projectile).Origin = origin;
        _guns.ShootProjectile(projectile, direction, Vector2.Zero, ent, ent, 30);
        _plasma.ChangePlasmaAmount(ent, -30);
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphSpit", AudioParams.Default.WithVolume(-10)), ent);
        args.Handled = true;
    }
    private void OnSpitHit(Entity<XenomorphToxicProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!TryComp<ProjectileComponent>(ent, out var projectile) || projectile.Shooter is not { } shooter
            || !TryComp<XenomorphSpitProjectileComponent>(ent, out var spit) || spit.Origin is not { } origin
            || (_transform.GetMapCoordinates(ent).Position - origin.Position).LengthSquared() > 49) return;
        if (AddIntoxication(shooter, args.Target, 5)) HitSound(args.Target);
    }
    private void HitSound(EntityUid target) => _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/spray3.ogg", AudioParams.Default.WithVolume(-4)), target);
    private void OnSlash(Entity<XenomorphToxicComponent> ent, ref XenomorphToxicSlashActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 100)) return;
        _plasma.ChangePlasmaAmount(ent, -100);
        ent.Comp.HitsRemaining = 3;
        ent.Comp.SlashUntil = ent.Comp.SpeedUntil = _timing.CurTime + TimeSpan.FromSeconds(4);
        ent.Comp.SlashAction = args.Action.Owner;
        Dirty(ent);
        _actions.SetToggled(args.Action.Owner, true);
        _movement.RefreshMovementSpeedModifiers(ent);
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoDrool", AudioParams.Default.WithVolume(-4)), ent);
        args.Handled = true;
    }
    private void EndSlash(Entity<XenomorphToxicComponent> ent)
    {
        ent.Comp.HitsRemaining = 0;
        ent.Comp.SlashUntil = TimeSpan.Zero;
        Dirty(ent);
        if (ent.Comp.SlashAction is { } action) _actions.SetToggled(action, false);
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoTailSwipe", AudioParams.Default.WithVolume(-8)), ent);
    }
    private void OnMelee(Entity<XenomorphToxicComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || ent.Comp.HitsRemaining <= 0 || ent.Comp.SlashUntil <= _timing.CurTime) return;
        foreach (var target in args.HitEntities)
        {
            if (!AddIntoxication(ent, target, 5)) continue;
            HitSound(target);
            if (--ent.Comp.HitsRemaining == 0) EndSlash(ent);
            else Dirty(ent);
            break;
        }
    }
    private void OnSting(Entity<XenomorphToxicComponent> ent, ref XenomorphDrainStingActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !Victim(args.Target) || !_plasma.HasPlasma(ent, 75)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f)
            || !TryComp<XenomorphIntoxicatedComponent>(args.Target, out var toxin) || toxin.Stacks <= 0) return;
        _plasma.ChangePlasmaAmount(ent, -75);
        var stacks = toxin.Stacks;
        var potency = stacks * 6;
        _damage.TryChangeDamage(args.Target, new DamageSpecifier { DamageDict = { ["Caustic"] = potency / 5f } }, origin: ent);
        _stun.TryKnockdown(args.Target, TimeSpan.FromSeconds(Math.Max(.1f, (stacks - 10) / 10f)), true);
        if (TryComp<DamageableComponent>(ent, out var damage) && damage.TotalDamage > 0)
        {
            var heal = new DamageSpecifier();
            foreach (var (type, value) in damage.Damage.DamageDict)
                if (value > 0) heal.DamageDict[type] = -Math.Min(value.Float(), value.Float() / damage.TotalDamage.Float() * potency);
            _damage.TryChangeDamage(ent, heal, ignoreResistances: true, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
        }
        _plasma.ChangePlasmaAmount(ent, potency * 3.5f);
        toxin.Stacks = Math.Max(0, stacks - Math.Max(1, (int)MathF.Round(stacks * .7f, MidpointRounding.AwayFromZero)));
        RefreshIntoxication((args.Target, toxin));
        if (toxin.Stacks <= 0) RemComp<XenomorphIntoxicatedComponent>(args.Target);
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoTailSwipe", AudioParams.Default.WithVolume(-4)), args.Target);
        if (stacks >= 20)
        {
            var surge = EnsureComp<XenomorphDrainSurgeComponent>(ent);
            surge.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(12);
            Dirty(ent, surge);
            _armor.RefreshAlert(ent);
            _audio.PlayPvs(new SoundCollectionSpecifier("XenoDrool"), args.Target);
            var target = args.Target;
            Timer.Spawn(TimeSpan.FromSeconds(.5), () => { if (Exists(target)) _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_bite2.ogg"), target); });
        }
        args.Handled = true;
    }
    private bool Corrodible(EntityUid target) => !HasComp<MobStateComponent>(target)
        && (HasComp<ItemComponent>(target) || HasComp<Content.Shared._White.Other.StructureComponent>(target))
        && !HasComp<XenomorphAciderCorrosionComponent>(target) && !HasComp<Content.Shared._White.Xenomorphs.Acid.Components.AcidCorrodingComponent>(target);
    private void OnCorrode(Entity<XenomorphToxicComponent> ent, ref XenomorphWeakCorrodeActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 75) || !Corrodible(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 2)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 4, new XenomorphWeakCorrodeDoAfterEvent(), ent, args.Target)
        { BreakOnMove = true, NeedHand = false, DistanceThreshold = 2 });
    }
    private void OnCorrodeDone(Entity<XenomorphToxicComponent> ent, ref XenomorphWeakCorrodeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target || !Ready(ent) || !Corrodible(target)
            || !_interaction.InRangeUnobstructed(ent.Owner, target, range: 2) || !_plasma.HasPlasma(ent, 75)) return;
        _plasma.ChangePlasmaAmount(ent, -75);
        _corrosion.Corrode(target, 4, 125, false, "DumontXenomorphWeakCorrosiveAcid");
        args.Handled = true;
    }
    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var toxic = EntityQueryEnumerator<XenomorphToxicComponent>();
        while (toxic.MoveNext(out var uid, out var comp))
        {
            if (comp.SlashUntil != TimeSpan.Zero && now >= comp.SlashUntil) EndSlash((uid, comp));
            if (comp.SpeedUntil != TimeSpan.Zero && now >= comp.SpeedUntil)
            {
                comp.SpeedUntil = TimeSpan.Zero;
                Dirty(uid, comp);
                _movement.RefreshMovementSpeedModifiers(uid);
            }
        }
        var surges = EntityQueryEnumerator<XenomorphDrainSurgeComponent>();
        while (surges.MoveNext(out var uid, out var surge))
            if (now >= surge.ExpiresAt) { RemComp<XenomorphDrainSurgeComponent>(uid); _armor.RefreshAlert(uid); }
        var projectiles = EntityQueryEnumerator<XenomorphToxicProjectileComponent, XenomorphSpitProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out _, out var spit))
            if (spit.Origin is { } origin && (_transform.GetMapCoordinates(uid).Position - origin.Position).LengthSquared() > 49) QueueDel(uid);
        UpdateIntoxication();
    }
}
