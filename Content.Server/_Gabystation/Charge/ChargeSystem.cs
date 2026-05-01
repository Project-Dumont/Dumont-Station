// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Destructible;
using Content.Goobstation.Shared.Dash;
using Content.Shared._Gabystation.Charge;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Server._Gabystation.Charge;

public sealed class ChargeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargeComponent, DashActionEvent>(OnDashAction);
        SubscribeLocalEvent<ChargeComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<ChargeComponent, ThrowDoHitEvent>(OnThrowDoHit);
        SubscribeLocalEvent<ChargeComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnDashAction(Entity<ChargeComponent> ent, ref DashActionEvent args)
    {
        if (args.Performer != ent.Owner)
            return;

        if (MetaData(args.Action).EntityPrototype?.ID is not { } actionId)
            return;

        if (actionId != ent.Comp.ChargeAction)
            return;

        ent.Comp.PendingCharge = true;
    }

    private void OnThrown(Entity<ChargeComponent> ent, ref ThrownEvent args)
    {
        if (!ent.Comp.PendingCharge)
            return;

        ent.Comp.PendingCharge = false;
        ent.Comp.IsCharging = true;
        ent.Comp.HitDuringCurrentCharge.Clear();
    }

    private void OnThrowDoHit(Entity<ChargeComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!ent.Comp.IsCharging)
            return;

        if (args.Target == ent.Owner)
            return;

        if (HasComp<MobStateComponent>(args.Target))
        {
            if (AlreadyHitThisCharge(ent.Comp, args.Target))
                return;

            _damageable.TryChangeDamage(args.Target, ent.Comp.TargetDamage, origin: ent.Owner);
            _stun.TryKnockdown(args.Target, ent.Comp.TargetKnockdown, true, true, false);
            return;
        }

        if (HasComp<DestructibleComponent>(args.Target))
        {
            if (AlreadyHitThisCharge(ent.Comp, args.Target))
                return;

            _damageable.TryChangeDamage(args.Target, ent.Comp.FragileDamage, origin: ent.Owner);

            if (ent.Comp.StopOnFragileHit)
            {
                EndChargeWithSelfKnockdown(ent);
                return;
            }

            if (ent.Comp.KnockdownOnFragileHit)
                _stun.TryKnockdown(ent.Owner, ent.Comp.WallKnockdown, true, true, false);

            return;
        }

        if (!TryComp(args.Target, out PhysicsComponent? physics))
            return;

        if (physics.BodyType != BodyType.Static)
            return;

        EndChargeWithSelfKnockdown(ent);
    }

    private void EndChargeWithSelfKnockdown(Entity<ChargeComponent> ent)
    {
        ResetChargeState(ent.Comp);
        _stun.TryKnockdown(ent.Owner, ent.Comp.WallKnockdown, true, true, false);
    }

    private void OnStopThrow(Entity<ChargeComponent> ent, ref StopThrowEvent args)
    {
        if (!ent.Comp.PendingCharge &&
            !ent.Comp.IsCharging &&
            ent.Comp.HitDuringCurrentCharge.Count == 0)
            return;

        ResetChargeState(ent.Comp);
    }

    private static bool AlreadyHitThisCharge(ChargeComponent comp, EntityUid target)
    {
        return !comp.HitDuringCurrentCharge.Add(target);
    }

    private static void ResetChargeState(ChargeComponent comp)
    {
        comp.PendingCharge = false;
        comp.IsCharging = false;
        comp.HitDuringCurrentCharge.Clear();
    }
}
