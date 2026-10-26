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
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphHeadbuttSystem : EntitySystem
{
    private static readonly SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg");
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private ThrownItemSystem _thrownItem = default!;
    [Dependency] private TransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHeadbuttComponent, XenomorphHeadbuttActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphHeadbuttComponent, ThrowDoHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphHeadbuttComponent, StopThrowEvent>(OnStop);
    }

    private void OnAction(Entity<XenomorphHeadbuttComponent> ent, ref XenomorphHeadbuttActionEvent args)
    {
        if (args.Handled || ent.Comp.Charging || !_mobState.IsAlive(ent)
            || !_blocker.CanAttack(ent, args.Target) || !_blocker.CanMove(ent)
            || _container.IsEntityInContainer(ent) || Transform(ent).Anchored
            || HasComp<XenomorphComponent>(args.Target) || !HasComp<MobStateComponent>(args.Target)
            || !_plasma.HasPlasma(ent, 10) || !TryComp<PhysicsComponent>(ent, out var physics))
            return;

        var crested = TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered;
        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: crested ? 1.5f : ent.Comp.Range))
            return;

        var offset = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        if (offset.LengthSquared() < 0.01f)
            return;

        _physics.ResetDynamics(ent, physics);
        _throwing.TryThrow(ent, Vector2.Normalize(offset) * ent.Comp.Range, user: ent, pushbackRatio: 0, doSpin: false);
        if (!HasComp<ThrownItemComponent>(ent))
            return;

        ent.Comp.Charging = true;
        args.Handled = true;
    }

    private void OnHit(Entity<XenomorphHeadbuttComponent> ent, ref ThrowDoHitEvent args)
    {
        if (args.Handled || !ent.Comp.Charging)
            return;

        args.Handled = true;
        if (HasComp<XenomorphComponent>(args.Target))
            return;

        _thrownItem.StopThrow(ent, args.Component);
        if (!HasComp<MobStateComponent>(args.Target) || !_mobState.IsAlive(ent))
            return;

        var crested = TryComp<XenomorphCrestComponent>(ent, out var crest) && crest.Lowered;
        var damage = new DamageSpecifier { DamageDict = { ["Blunt"] = crested ? ent.Comp.CrestedDamage : ent.Comp.Damage } };
        _damage.TryChangeDamage(args.Target, damage, origin: ent);
        _audio.PlayPvs(HitSound, args.Target);
        var offset = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        if (!Transform(args.Target).Anchored && offset.LengthSquared() > 0)
            _throwing.TryThrow(args.Target, Vector2.Normalize(offset) * (crested ? ent.Comp.CrestedThrowDistance : ent.Comp.ThrowDistance),
                user: ent, doSpin: false);
    }

    private void OnStop(Entity<XenomorphHeadbuttComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.Charging = false;
    }
}
