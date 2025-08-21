// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kill_Me_I_Noobs <118206719+Vonsant@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxNext.CrewMedal;

/// <summary>
/// Enum representing the key for the Crew Medal user interface.
/// </summary>
[Serializable, NetSerializable]
public enum CrewMedalUiKey : byte
{
    Key
}

/// <summary>
/// Message sent when the reason for the medal is changed via the user interface.
/// </summary>
[Serializable, NetSerializable]
public sealed class CrewMedalReasonChangedMessage(string Reason) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The new reason for the medal.
    /// </summary>
    public string Reason { get; } = Reason;
}
