// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._CorvaxGoob.Animation;
using Content.Shared._CorvaxGoob.Events.Animation;
using Content.Shared._CorvaxGoob.Events.StatusEffects;
using Content.Shared.StatusEffect;

namespace Content.Server._CorvaxGoob.Events;

public sealed class TargetEventsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayAnimationTargetEvent>(OnPlayAnimation);
        SubscribeLocalEvent<ApplyStatusEffectTargetEvent>(OnApplyStatusEffect);
    }

    private void OnPlayAnimation(PlayAnimationTargetEvent ev)
    {
        _animation.PlayAnimation(ev.Target, ev.AnimationID);
    }

    private void OnApplyStatusEffect(ApplyStatusEffectTargetEvent ev)
    {
        if (ev.ComponentType is null)
            return;

        _statusEffects.TryAddStatusEffect(ev.Target, ev.Key, TimeSpan.FromSeconds(ev.Time), ev.Refresh, ev.ComponentType);
    }
}
