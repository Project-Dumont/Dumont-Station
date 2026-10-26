using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Effects;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphStrikeSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedColorFlashEffectSystem _flash = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private TransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphComponent, XenomorphStrikeActionEvent>(OnStrike);
    }

    private void OnStrike(Entity<XenomorphComponent> ent, ref XenomorphStrikeActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent) || HasComp<XenomorphComponent>(args.Target)
            || !HasComp<MobStateComponent>(args.Target) || _mobs.IsDead(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
            return;

        _pulling.StopAllPulls(args.Target);
        if (_damage.TryChangeDamage(args.Target, args.Damage, origin: ent)?.GetTotal() > 0)
            _flash.RaiseEffect(Color.Red, new List<EntityUid> { args.Target }, Filter.Pvs(args.Target, entityManager: EntityManager));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg"), ent);
        Spawn("DumontXenomorphPunchEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        var offset = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        var local = (-_transform.GetWorldRotation(ent)).RotateVec(offset);
        _melee.DoLunge(ent, ent, ent, Angle.Zero, local, "WeaponArcThrust", Angle.Zero, false, predicted: false);
        if (args.Slow > TimeSpan.Zero && TryComp<MovementSpeedModifierComponent>(args.Target, out var speed))
            _slow.TryUpdateMovementSpeedModDuration(args.Target, "XenomorphStompSlowdown", args.Slow,
                1 / (1 + Math.Max(speed.CurrentSprintSpeed, speed.BaseSprintSpeed) * 0.8f));
        if (args.Knockdown > TimeSpan.Zero)
            _stun.TryUpdateParalyzeDuration(args.Target, args.Knockdown);

        if (args.ThrowDistance > 0 && !Transform(args.Target).Anchored)
        {
            var direction = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
            if (direction.LengthSquared() > 0)
                _throwing.TryThrow(args.Target, Vector2.Normalize(direction) * args.ThrowDistance, args.ThrowSpeed, user: ent);
        }

        args.Handled = true;
    }
}
