// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.GameTicking;
using Content.Shared.DeltaV.CCVars;
using Robust.Shared.Configuration;

namespace Content.Client.DeltaV.RoundEnd;

public sealed class NoEorgPopupSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private NoEorgPopup? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (_cfg.GetCVar(DCCVars.SkipRoundEndNoEorgPopup) || _cfg.GetCVar(DCCVars.RoundEndNoEorgPopup) == false)
            return;

        OpenNoEorgPopup();
    }

    private void OpenNoEorgPopup()
    {
        if (_window != null)
            return;

        _window = new NoEorgPopup();
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
