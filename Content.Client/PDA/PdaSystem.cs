// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.PDA;
using Content.Shared.Popups;
using Robust.Shared.GameStates;

namespace Content.Client.PDA;

public sealed class PdaSystem : SharedPdaSystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PdaComponent, ComponentHandleState>(OnHandleState);
    }

    public void OnHandleState(Entity<PdaComponent> pda, ref ComponentHandleState msg) 
    {
        if (!IsPdaAttachedToPlayer(pda))
            return;

        _popup.PopupClient(Loc.GetString("pda-new-notification"), pda.Owner, PopupType.Medium);
    }
}