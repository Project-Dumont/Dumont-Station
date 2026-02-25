// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kill_Me_I_Noobs <118206719+Vonsant@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Will Oliver <willyangame76@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxNext.CrewMedal;

/// <summary>
/// Component for a medal that can be awarded to a player and
/// will be displayed in the final round summary screen.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class CrewMedalComponent : Component
{
    /// <summary>
    /// The name of the recipient of the award.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public string Recipient = string.Empty;

    /// <summary>
    /// The reason for the award. Can be set before the medal is awarded.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public string Reason = string.Empty;

    /// <summary>
    /// If <c>true</c>, the medal is considered awarded, and the reason can no longer be changed.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public bool Awarded;

    /// <summary>
    /// The maximum number of characters allowed for the reason.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public int MaxCharacters = 50;
}
