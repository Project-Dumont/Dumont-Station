// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.LastWords;

/// <summary>
/// Tracks the last words a user has said.
/// </summary>
[RegisterComponent]
public sealed partial class LastWordsComponent : Component
{
    [DataField]
    public string? LastWords;
}
