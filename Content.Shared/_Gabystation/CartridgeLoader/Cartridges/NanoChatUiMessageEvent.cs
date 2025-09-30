// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public enum NanoBankUiMessageType : byte
{
    Empty,
    Login,
    Logout,
    Transfer,
    ToggleMute,
}

// Separar em diferentes messages
[Serializable, NetSerializable]
public sealed class NanoBankUiMessageEvent : CartridgeMessageEvent
{
    public readonly NanoBankUiMessageType Type;

    public readonly int? TargetAccount;

    public readonly int? Content;
    public NanoBankUiMessageEvent(NanoBankUiMessageType type,
        int? targetAccount = null,
        int? content = null)
    {
        Type = type;
        TargetAccount = targetAccount;
        Content = content;
    }
}
