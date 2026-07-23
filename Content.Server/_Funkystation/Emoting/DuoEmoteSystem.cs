// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Emoting;
using Content.Shared._Funkystation.Emoting.Components;
using Content.Shared._Funkystation.Emoting.EntitySystems;

namespace Content.Server._Funkystation.Emoting;

public sealed class ServerDuoEmoteSystem : SharedDuoEmoteSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<DuoEmoteAttemptEvent>(OnDuoEmoteAttempt);
    }

    private void OnDuoEmoteAttempt(DuoEmoteAttemptEvent msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession.AttachedEntity;
        if (player == null)
            return;

        if (!TryComp<DuoEmoteComponent>(player, out var selfComp))
            return;

        var targetUid = GetEntity(msg.Target);

        if (!TryComp<DuoEmoteComponent>(targetUid, out var targetComp))
            return;

        // neither should already be active
        if (selfComp.Active || targetComp.Active)
            return;

        // range check
        if (!_interaction.InRangeUnobstructed(player.Value, targetUid, selfComp.InteractRange))
        {
            return;
        }

        AttemptDuoEmote((player.Value, selfComp), (targetUid, targetComp), msg.EmoteId);
    }
}
