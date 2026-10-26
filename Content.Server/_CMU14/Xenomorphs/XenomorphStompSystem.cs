using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphStompSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MovementModStatusSystem _movement = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private TransformSystem _transform = default!;

    private static readonly EntProtoId Slowdown = "XenomorphStompSlowdown";
    private readonly HashSet<Entity<MobStateComponent>> _targets = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphStompComponent, XenomorphStompActionEvent>(OnStomp);
    }

    private void OnStomp(Entity<XenomorphStompComponent> ent, ref XenomorphStompActionEvent args)
    {
        if (args.Handled || !_mobState.IsAlive(ent) || !_blocker.CanAttack(ent))
            return;

        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        var length = direction.Length();
        if (origin.MapId != target.MapId || !float.IsFinite(length) || length < 0.1f
            || !_plasma.ChangePlasmaAmount(ent, -FixedPoint2.New(ent.Comp.PlasmaCost)))
            return;

        args.Handled = true;
        _audio.PlayPvs(ent.Comp.Sound, ent);
        var facing = direction.ToWorldAngle();
        var halfAngle = ent.Comp.Angle.Theta / 2;
        _targets.Clear();
        _lookup.GetEntitiesInRange(origin, ent.Comp.Range, _targets);
        foreach (var victim in _targets)
        {
            if (HasComp<XenomorphComponent>(victim) || _mobState.IsDead(victim))
                continue;

            var offset = _transform.GetMapCoordinates(victim).Position - origin.Position;
            var distance = offset.Length();
            if (distance < 0.1f || distance > ent.Comp.Range
                || Math.Abs(Angle.ShortestDistance(facing, offset.ToWorldAngle()).Theta) > halfAngle
                || !_interaction.InRangeUnobstructed(ent.Owner, victim.Owner, range: ent.Comp.Range))
                continue;

            var damage = ent.Comp.Damage + (ent.Comp.MinDamage - ent.Comp.Damage) * distance / ent.Comp.Range;
            _damage.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { ["Blunt"] = FixedPoint2.New(damage) } }, origin: ent);
            _stun.TryUpdateParalyzeDuration(victim, ent.Comp.ParalyzeTime);
            if (TryComp<MovementSpeedModifierComponent>(victim, out var movement))
            {
                var speed = Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed);
                _movement.TryUpdateMovementSpeedModDuration(victim, Slowdown, ent.Comp.SlowTime, 1 / (1 + speed * 0.4f));
            }

            if (ent.Comp.KnockbackDistance > 0 && !Transform(victim).Anchored)
                _throwing.TryThrow(victim, Vector2.Normalize(offset) * ent.Comp.KnockbackDistance, 10, user: ent, doSpin: false);
        }
    }
}
