// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Devour;
using Content.Goobstation.Shared.Devour.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Devour.Systems;

public sealed class PreventSelfRevivalSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventSelfRevivalComponent, BeforeSelfRevivalEvent>(OnAttemptSelfRevive);
    }

    private void OnAttemptSelfRevive(Entity<PreventSelfRevivalComponent> ent, ref BeforeSelfRevivalEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString(args.PopupText), args.Target, args.Target, PopupType.SmallCaution);
        args.Cancelled = true;
    }
}
