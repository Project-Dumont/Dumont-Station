// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Destructible;
using Content.Goobstation.Shared.Dash;
using Content.Shared._Gabystation.OniCharge;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Events;

namespace Content.Server._Gabystation.OniCharge;

public sealed class OniChargeSystem : EntitySystem
{
    private const string OniChargeActionPrototype = "ActionOniCharge";

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OniChargeComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<OniChargeComponent, DashActionEvent>(OnDashAction);
        SubscribeLocalEvent<OniChargeComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<OniChargeComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<OniChargeComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnDashAction(Entity<OniChargeComponent> ent, ref DashActionEvent args)
    {
        if (args.Performer != ent.Owner)
            return;

        if (MetaData(args.Action).EntityPrototype?.ID != OniChargeActionPrototype)
            return;

        ent.Comp.PendingCharge = true;
    }

    private void OnThrown(Entity<OniChargeComponent> ent, ref ThrownEvent args)
    {
        if (!ent.Comp.PendingCharge)
            return;

        ent.Comp.PendingCharge = false;
        ent.Comp.IsCharging = true;
        ent.Comp.HitDuringCurrentCharge.Clear();
    }

    private void OnStartCollide(Entity<OniChargeComponent> ent, ref StartCollideEvent args)
    {
        if (!HasComp<ThrownItemComponent>(ent))
            return;

        if (!ent.Comp.IsCharging)
            return;

        if (HasComp<MobStateComponent>(args.OtherEntity))
        {
            if (!ent.Comp.HitDuringCurrentCharge.Add(args.OtherEntity))
                return;

            var damage = new DamageSpecifier
            {
                DamageDict = { { "Blunt", ent.Comp.TargetBluntDamage } }
            };

            _damageable.TryChangeDamage(args.OtherEntity, damage, origin: ent.Owner);
            _stun.TryKnockdown(args.OtherEntity, ent.Comp.TargetKnockdown, true, true, false);
            return;
        }

        if (HasComp<DestructibleComponent>(args.OtherEntity))
        {
            var damage = new DamageSpecifier
            {
                DamageDict = { { "Structural", ent.Comp.FragileObstacleDamage } }
            };

            _damageable.TryChangeDamage(args.OtherEntity, damage, origin: ent.Owner);
        }

        _stun.TryKnockdown(ent.Owner, ent.Comp.WallKnockdown, true, true, false);
    }

    private void OnLand(Entity<OniChargeComponent> ent, ref LandEvent args)
    {
        ent.Comp.PendingCharge = false;
        ent.Comp.IsCharging = false;
        ent.Comp.HitDuringCurrentCharge.Clear();

        if (!TryComp<StaminaComponent>(ent.Owner, out var stamina))
            return;

        if (stamina.StaminaDamage < stamina.CritThreshold)
            return;

        _stun.TryKnockdown(ent.Owner, ent.Comp.ExhaustedKnockdown, true, true, false);
    }

    private void OnStopThrow(Entity<OniChargeComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.PendingCharge = false;
        ent.Comp.IsCharging = false;
        ent.Comp.HitDuringCurrentCharge.Clear();
    }
}
