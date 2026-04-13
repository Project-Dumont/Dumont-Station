// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat;
using Content.Shared._Gabystation.Speech.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Gabystation.Speech.EntitySystems;

public sealed class SpeakDatasetOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeakDatasetOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<SpeakDatasetOnUseComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || !_prototypeManager.TryIndex(ent.Comp.Dataset, out var speechLocalization) || speechLocalization.Values.Count == 0)
            return;

        _chat.TrySendInGameICMessage(args.User, _random.Pick(speechLocalization), InGameICChatType.Speak, false);
        args.Handled = true;
    }
}
