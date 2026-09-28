// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;
// Dumont start
using Content.Server.Medical;
using Content.Shared.Emoting;
using Content.Shared.StatusEffectNew;
using Content.Trauma.Shared.StatusEffects;
// Dumont end

namespace Content.Goobstation.Server.Emoting;

public sealed partial class AnimatedEmotesSystem : SharedAnimatedEmotesSystem
{
    // Dumont start
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private VomitSystem _vomit = default!;
    private static readonly EntProtoId EmoteCounter = "EmoteVomitCounterStatusEffect";
    private static readonly EntProtoId EmoteBlock = "BlockVomitEmotesStatusEffect";
    // Dumont end
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
        // Dumont start
        SubscribeLocalEvent<AnimatedEmotesComponent, BeforeEmoteEvent>(OnBeforeEmote);
        // Dumont end
    }

    // Dumont start
    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.ID is "Flip" or "Spin" && _status.HasStatusEffect(ent, EmoteBlock))
            args.Cancel();
    }
    // Dumont end

    private void OnEmote(EntityUid uid, AnimatedEmotesComponent component, ref EmoteEvent args)
    {
        // Dumont start
        if (args.Emote.ID is "Flip" or "Spin")
        {
            if (_status.HasStatusEffect(uid, EmoteBlock))
                return;

            if (_status.TryUpdateStatusEffectDuration(uid, EmoteCounter, out var effect, TimeSpan.FromSeconds(1)))
            {
                var counter = EnsureComp<CounterStatusEffectComponent>(effect.Value);
                counter.Count++;
                Dirty(effect.Value, counter);
                if (counter.Count >= 5)
                {
                    _status.TryUpdateStatusEffectDuration(uid, EmoteBlock, TimeSpan.FromSeconds(10));
                    _vomit.Vomit(uid, -8f, -8f);
                    return;
                }
            }
        }
        // Dumont end
        PlayEmoteAnimation(uid, component, args.Emote.ID);

        if (args.Emote.TargetEvents is not null) // CorvaxGoob-PrototypedAnimations : Raise to play client prototyped animation if it's exist
            foreach (var targetEvent in args.Emote.TargetEvents)
            {
                targetEvent.Target = uid;
                RaiseLocalEvent(uid, (object) targetEvent, true);
            }
    }

    public void PlayEmoteAnimation(EntityUid uid, AnimatedEmotesComponent component, ProtoId<EmotePrototype> prot)
    {
        component.Emote = prot;
        Dirty(uid, component);
    }
}
