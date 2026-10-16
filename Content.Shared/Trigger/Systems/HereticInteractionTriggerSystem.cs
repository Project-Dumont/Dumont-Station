// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Throwing;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared.Trigger.Systems;

// Dumont start
public sealed class HereticInteractionTriggerSystem : TriggerOnXSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TriggerOnThrownComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<TriggerOnGotEquippedHandComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<TriggerOnMeleeHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnThrown(Entity<TriggerOnThrownComponent> ent, ref ThrownEvent args)
        => Trigger.Trigger(ent, args.User, ent.Comp.KeyOut);

    private void OnEquipped(Entity<TriggerOnGotEquippedHandComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!_timing.ApplyingState)
            Trigger.Trigger(ent, args.User, ent.Comp.KeyOut);
    }

    private void OnMeleeHit(Entity<TriggerOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            Trigger.Trigger(ent, ent.Comp.TargetIsUser ? target : args.User, ent.Comp.KeyOut);
            if (!ent.Comp.TriggerEveryHit)
                break;
        }
    }
}
// Dumont end
