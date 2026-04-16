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
using Robust.Shared.Physics.Events;

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
        SubscribeLocalEvent<ChargeComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ChargeComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<ChargeComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnDashAction(Entity<ChargeComponent> ent, ref DashActionEvent args)
    {
        if (args.Performer != ent.Owner)
            return;

        if (!IsChargeAction(args.Action, ent.Comp))
            return;

        ent.Comp.PendingCharge = true;
    }

    private void OnThrown(Entity<ChargeComponent> ent, ref ThrownEvent args)
    {
        if (!ent.Comp.PendingCharge)
            return;

        BeginCharge(ent.Comp);
    }

    private void OnThrowDoHit(Entity<ChargeComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!ent.Comp.IsCharging)
            return;

        if (args.Target == ent.Owner)
            return;

        if (TryHandleMobHit(ent, args.Target))
            return;

        TryHandleFragileHit(ent, args.Target);
    }

    private void OnStartCollide(Entity<ChargeComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsCharging)
            return;

        if (args.OtherEntity == ent.Owner)
            return;

        
        
        if (TryHandleMobHit(ent, args.OtherEntity))
            return;

        
        if (HasComp<DestructibleComponent>(args.OtherEntity))
            return;

        if (!args.OtherFixture.Hard)
            return;

        if (!TryComp(args.OtherEntity, out PhysicsComponent? physics))
            return;

        if (physics.BodyType != BodyType.Static)
            return;

        HandleWallImpact(ent);
    }

    private void OnLand(Entity<ChargeComponent> ent, ref LandEvent args)
    {
        EndCharge(ent.Comp);
    }

    private void OnStopThrow(Entity<ChargeComponent> ent, ref StopThrowEvent args)
    {
        EndCharge(ent.Comp);
    }

    private bool IsChargeAction(EntityUid action, ChargeComponent comp)
    {
        if (MetaData(action).EntityPrototype?.ID is not { } id)
            return false;

        return id == comp.ChargeAction;
    }

    private static void BeginCharge(ChargeComponent comp)
    {
        comp.PendingCharge = false;
        comp.IsCharging = true;
        comp.HitDuringCurrentCharge.Clear();
    }

    private static void EndCharge(ChargeComponent comp)
    {
        comp.PendingCharge = false;
        comp.IsCharging = false;
        comp.HitDuringCurrentCharge.Clear();
    }

    private bool TryHandleMobHit(Entity<ChargeComponent> ent, EntityUid target)
    {
        if (!HasComp<MobStateComponent>(target))
            return false;

        if (!ent.Comp.HitDuringCurrentCharge.Add(target))
            return true;

        _damageable.TryChangeDamage(target, ent.Comp.TargetDamage, origin: ent.Owner);
        _stun.TryKnockdown(target, ent.Comp.TargetKnockdown, true, true, false);
        return true;
    }

    private bool TryHandleFragileHit(Entity<ChargeComponent> ent, EntityUid target)
    {
        if (!HasComp<DestructibleComponent>(target))
            return false;

        if (!ent.Comp.HitDuringCurrentCharge.Add(target))
            return true;

        _damageable.TryChangeDamage(target, ent.Comp.FragileDamage, origin: ent.Owner);

        if (ent.Comp.StopOnFragileHit)
        {
            HandleWallImpact(ent);
            return true;
        }

        if (ent.Comp.KnockdownOnFragileHit)
            _stun.TryKnockdown(ent.Owner, ent.Comp.WallKnockdown, true, true, false);

        return true;
    }

    private void HandleWallImpact(Entity<ChargeComponent> ent)
    {
        EndCharge(ent.Comp);
        _stun.TryKnockdown(ent.Owner, ent.Comp.WallKnockdown, true, true, false);
    }
}
