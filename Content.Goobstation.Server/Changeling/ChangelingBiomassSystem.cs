// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Chat.Systems;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingBiomassSystem : SharedChangelingBiomassSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    protected override void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        _chat.TryEmoteWithChat(ent, ent.Comp.CoughEmote, ignoreActionBlocker: true, forceEmote: true);
    }
}
