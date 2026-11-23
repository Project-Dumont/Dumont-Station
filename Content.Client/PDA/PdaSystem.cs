// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Notification;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Robust.Shared.GameStates;

namespace Content.Client.PDA;

public sealed partial class PdaSystem : SharedPdaSystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotificationReceiverComponent, ComponentHandleState>(OnHandleState);
    }

    public void OnHandleState(Entity<NotificationReceiverComponent> receiver, ref ComponentHandleState state) 
    {
        if (!TryComp<PdaComponent>(receiver.Owner, out var pdaComp))
            return;

        if (!IsPdaAttachedToPlayer((receiver.Owner, pdaComp)))
            return;

        if (state.Current is not NotificationReceiverState currentState)
            return;

        if (state.Next is not NotificationReceiverState nextState)
            return;

        if (currentState.Notifications.Count == nextState.Notifications.Count)
            return;

        _popup.PopupClient(Loc.GetString("pda-new-notification"), receiver.Owner, PopupType.Medium);
    }
}