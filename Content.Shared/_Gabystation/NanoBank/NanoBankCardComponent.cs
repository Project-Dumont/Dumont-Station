// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.NanoBank;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NanoBankCardComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool LoggedIn = false;

    [DataField, AutoNetworkedField]
    public int AccountId = 0;

    [DataField]
    public int AccountPin = 0;

    [DataField, AutoNetworkedField]
    public bool NotificationsMuted = false;

    /// <summary>
    /// The station linked to the account.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Station;
}
