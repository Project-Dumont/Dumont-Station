using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Armor;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CMU14.Xenomorphs;
public sealed partial class XenomorphSpitSystem : EntitySystem
{
    [Dependency] private XenomorphSpitterSystem _spitter = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IRobustRandom _random = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphSpitComponent, XenomorphSpitActionEvent>(OnSpit);
        SubscribeLocalEvent<XenomorphSpitProjectileComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphSpitProjectileComponent, PreventCollideEvent>(OnCollide);
    }
    private void OnSpit(Entity<XenomorphSpitComponent> ent, ref XenomorphSpitActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent) || !args.Bone && !_plasma.HasPlasma(ent, args.Acid ? 25 : args.Scattered ? 30 : 20))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < 0.001f)
            return;
        var charged = args.Acid && TryComp<XenomorphSpitterComponent>(ent, out var charge) && charge.Charging && !charge.Fired;
        for (var i = 0; i < (args.Scattered ? 5 : 1); i++)
        {
            var angle = i == 0 ? Angle.Zero : _random.NextAngle(-Angle.FromDegrees(45), Angle.FromDegrees(45));
            var projectile = Spawn(args.Bone ? "DumontXenomorphBoneChips" : charged ? "DumontXenomorphChargedSpit" : args.Acid ? "DumontXenomorphAcidSpit" : args.Scattered ? "DumontXenomorphScatteredSpit" : "DumontXenomorphSlowingSpit", _transform.GetMoverCoordinates(ent));
            Comp<XenomorphSpitProjectileComponent>(projectile).Origin = origin;
            _guns.ShootProjectile(projectile, angle.RotateVec(direction), Vector2.Zero, ent, ent, args.Bone ? 20 : 30);
        }
        if (charged)
            Comp<XenomorphSpitterComponent>(ent).Fired = true;
        if (!args.Bone) _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphSpit", AudioParams.Default.WithVolume(-10)), ent);
        args.Handled = true;
    }
    private void OnCollide(Entity<XenomorphSpitProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<XenomorphComponent>(args.OtherEntity))
            return;
        args.Cancelled = true;
        if (ent.Comp.DeleteOnFriendly && !HasComp<XenomorphToxicProjectileComponent>(ent) && TryComp<ProjectileComponent>(ent, out var projectile) && projectile.Shooter != args.OtherEntity)
            QueueDel(ent);
    }
    private void OnHit(Entity<XenomorphSpitProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (ent.Comp.Suppressed) return;
        var target = args.Target;
        if (HasComp<XenomorphComponent>(target) || (ent.Comp.Origin is { } origin
            && (_transform.GetMapCoordinates(ent).Position - origin.Position).LengthSquared() > ent.Comp.Range * ent.Comp.Range))
        {
            args.Damage = new Content.Shared.Damage.DamageSpecifier();
            return;
        }
        if (HasComp<SiliconComponent>(target) || !HasComp<MobStateComponent>(target) || _mobs.IsDead(target))
            return;
        if (ent.Comp.Charged)
            _spitter.ApplyAcid(target);
        else if (ent.Comp.Acid)
            _spitter.ShowAcidSplash(target);
        if (ent.Comp.Slow > TimeSpan.Zero && TryComp<MovementSpeedModifierComponent>(target, out var speed))
            _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", ent.Comp.Slow,
                1 / (1 + Math.Max(speed.CurrentSprintSpeed, speed.BaseSprintSpeed) * ent.Comp.SlowFactor));
        var armor = new CoefficientQueryEvent(SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING);
        if (ent.Comp.ArmorResists)
            RaiseLocalEvent(target, armor);
        var protectedTarget = ent.Comp.ArmorResists && armor.DamageModifiers.Coefficients.Any(pair =>
            (pair.Key == "Piercing" || pair.Key == "Blunt") && pair.Value < 1);
        if (!protectedTarget && ent.Comp.Paralyze > TimeSpan.Zero)
            _stun.TryUpdateParalyzeDuration(target, ent.Comp.Paralyze);
        if (ent.Comp.DrainMedicine && _solutions.TryGetInjectableSolution(target, out var solution, out var contents)
            && solution != null && contents != null)
        {
            foreach (var reagent in contents.GetReagentPrototypes(_prototypes).Keys.ToArray())
                if (reagent.Group == "Medicine")
                    _solutions.RemoveReagent(solution.Value, reagent.ID, 2);
        }
    }
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphSpitProjectileComponent>();
        while (query.MoveNext(out var uid, out var spit))
        {
            if (spit.Origin is not { } origin)
                continue;
            var current = _transform.GetMapCoordinates(uid);
            if (current.MapId != origin.MapId || (current.Position - origin.Position).LengthSquared() >= spit.Range * spit.Range)
                QueueDel(uid);
        }
    }
}
