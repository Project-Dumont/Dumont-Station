// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;
using Content.Shared._Gabystation.CartridgeLoader.Cartridges;

namespace Content.Client._Gabystation.CartridgeLoader.Cartridges;

public sealed partial class NanoBankUi : UIFragment
{
    private NanoBankUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoBankUiFragment();

        _fragment.OnMessageSent += (type, targetAcc, content) =>
        {
            SendNanoBankUiMessage(type, targetAcc, content, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is NanoBankUiState cast)
            _fragment?.UpdateState(cast);
    }

    private static void SendNanoBankUiMessage(NanoBankUiMessageType type,
        int? targetAcc,
        int? content,
        BoundUserInterface userInterface)
    {
        var nanoBankMessage = new NanoBankUiMessageEvent(type, targetAcc, content);
        var message = new CartridgeUiMessage(nanoBankMessage);
        userInterface.SendMessage(message);
    }
}
