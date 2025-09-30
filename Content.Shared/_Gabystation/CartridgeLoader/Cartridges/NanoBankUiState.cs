// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoBankUiState : BoundUserInterfaceState
{
    public readonly int AccountId;
    public readonly int Pin;
    public readonly bool NotificationsMuted;
    public readonly bool Logged;
    public readonly float NextPayment;
    public readonly float Balance;

    public NanoBankUiState(
        int accountId,
        int pin,
        bool notificationsMuted,
        bool logged,
        float nextPayment,
        float balance)
    {
        AccountId = accountId;
        Pin = pin;
        NotificationsMuted = notificationsMuted;
        Logged = logged;
        NextPayment = nextPayment;
        Balance = balance;
    }
}
