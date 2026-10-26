using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphTailStabSystem : EntitySystem
{
    [Dependency] private XenomorphDespoilerSystem _despoiler = default!;
    [Dependency] private XenomorphSpitterSystem _spitter = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private RotateToFaceSystem _rotate = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphTailStabComponent, XenomorphTailStabActionEvent>(OnAction);
    }

    private void OnAction(Entity<XenomorphTailStabComponent> ent, ref XenomorphTailStabActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId == MapId.Nullspace || target.MapId != origin.MapId
            || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < 0.0001f)
            return;
        if (TryComp<MeleeWeaponComponent>(ent, out var weapon))
        {
            if (_timing.CurTime < weapon.NextAttack)
                return;
            weapon.NextAttack = _timing.CurTime + TimeSpan.FromSeconds(1);
            Dirty(ent, weapon);
        }
        direction = Vector2.Normalize(direction);
        var side = new Vector2(-direction.Y, direction.X) * 0.1f;
        bool Ignored(EntityUid uid) => uid == ent.Owner || !HasComp<MobStateComponent>(uid) || HasComp<XenomorphComponent>(uid);
        var left = new CollisionRay(origin.Position + side, direction, SharedMeleeWeaponSystem.AttackMask);
        var right = new CollisionRay(origin.Position - side, direction, SharedMeleeWeaponSystem.AttackMask);
        var hits = _physics.IntersectRayWithPredicate(origin.MapId, left, ent.Comp.Range, Ignored, false)
            .Concat(_physics.IntersectRayWithPredicate(origin.MapId, right, ent.Comp.Range, Ignored, false))
            .Select(h => h.HitEntity).Distinct()
            .OrderBy(uid => (_transform.GetMapCoordinates(uid).Position - origin.Position).LengthSquared());
        EntityUid? victim = null;
        foreach (var hit in hits)
        {
            if (!_interaction.InRangeUnobstructed(ent.Owner, hit, range: ent.Comp.Range))
                continue;
            victim = hit;
            break;
        }
        var hitEntities = victim is { } hitUid ? new List<EntityUid> { hitUid } : new List<EntityUid>();
        var tailBonus = victim is { } tailVictim && TryComp<XenomorphDespoilerComponent>(ent, out var despoiler)
            ? _despoiler.TailBonus((ent, despoiler), tailVictim) : new DamageSpecifier();
        var tailDamage = new XenomorphTailDamageEvent(tailBonus);
        RaiseLocalEvent(ent, ref tailDamage);
        tailBonus = tailDamage.Damage;
        var hitEvent = new MeleeHitEvent(hitEntities, ent, ent, ent.Comp.Damage, direction, args.Target);
        RaiseLocalEvent(ent, hitEvent);
        var damaged = false;
        if (victim is { } victimUid)
        {
            if (!hitEvent.Handled)
            {
                _interaction.DoContactInteraction(ent, ent);
                _interaction.DoContactInteraction(ent, victimUid);
                var attacked = new AttackedEvent(ent, ent, args.Target);
                RaiseLocalEvent(victimUid, attacked);
                var damage = DamageSpecifier.ApplyModifierSets(ent.Comp.Damage + tailBonus + hitEvent.BonusDamage + attacked.BonusDamage, hitEvent.ModifiersList);
                damaged = _damage.TryChangeDamage(victimUid, damage, origin: ent)?.GetTotal() > 0;
            }
            if (ent.Comp.Corrosive)
                _spitter.InjectAcid(victimUid, ent.Comp.AcidAmount);
            Spawn(ent.Comp.HitEffect, new EntityCoordinates(victimUid, Vector2.Zero));
            var xform = Transform(ent);
            var localPos = (-_transform.GetWorldRotation(xform)).RotateVec(direction * ent.Comp.Range);
            _melee.DoLunge(ent, ent, ent, Angle.Zero, localPos, "WeaponArcThrust", Angle.Zero, false, predicted: false);
            args.Handled = true;
            if (damaged)
            {
                _flash.RaiseEffect(Color.Red, new List<EntityUid> { victimUid }, Filter.Pvs(victimUid, entityManager: EntityManager));
                _rotate.TryFaceAngle(ent, _transform.GetWorldRotation(ent).GetDir().ToAngle() - Angle.FromDegrees(180));
            }
        }
        else
            _actions.SetCooldown(args.Action.Owner, TimeSpan.FromSeconds(ent.Comp.MissCooldown));
        _audio.PlayPvs(damaged ? ent.Comp.HitSound : ent.Comp.MissSound, ent);
        var attack = new MeleeAttackEvent(ent);
        RaiseLocalEvent(ent, ref attack);
    }
}
