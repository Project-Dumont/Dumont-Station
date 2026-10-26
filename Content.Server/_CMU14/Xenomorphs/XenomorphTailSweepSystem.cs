using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphTailSweepSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private TransformSystem _transform = default!;
    private readonly HashSet<Entity<MobStateComponent>> _targets = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphTailSweepComponent, XenomorphTailSweepActionEvent>(OnAction);
    }

    private void OnAction(Entity<XenomorphTailSweepComponent> ent, ref XenomorphTailSweepActionEvent args)
    {
        if (args.Handled || !_mobState.IsAlive(ent) || !_blocker.CanAttack(ent)
            || TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered
            || !_plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaCost))
            return;

        args.Handled = true;
        EnsureComp<XenomorphSweepingComponent>(ent);
        _audio.PlayPvs(ent.Comp.Sound, ent);
        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, _targets);
        foreach (var victim in _targets)
        {
            if (HasComp<XenomorphComponent>(victim) || _mobState.IsDead(victim)
                || !_interaction.InRangeUnobstructed(ent.Owner, victim.Owner, range: ent.Comp.Range))
                continue;

            if (TryComp<PullableComponent>(victim, out var pullable))
                _pulling.TryStopPull(victim, pullable, ignoreGrab: true);
            if (TryComp<PullerComponent>(victim, out var puller) && puller.Pulling is { } pulled
                && TryComp<PullableComponent>(pulled, out var pulledComp))
                _pulling.TryStopPull(pulled, pulledComp, ignoreGrab: true);

            _damage.TryChangeDamage(victim, ent.Comp.Damage, origin: ent);
            _stun.TryUpdateParalyzeDuration(victim, ent.Comp.ParalyzeTime);
            _audio.PlayPvs(ent.Comp.HitSound, victim);
            var offset = _transform.GetWorldPosition(victim) - _transform.GetWorldPosition(ent);
            if (ent.Comp.KnockbackDistance > 0 && !Transform(victim).Anchored && offset.LengthSquared() > 0)
                _throwing.TryThrow(victim, Vector2.Normalize(offset) * ent.Comp.KnockbackDistance, user: ent, doSpin: false);
        }
    }
}
