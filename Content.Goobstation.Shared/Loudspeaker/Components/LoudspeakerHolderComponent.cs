// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Loudspeaker.Components;

/// <summary>
///     Marks an entity that is holding equipped loudspeaker(s).
/// </summary>
[RegisterComponent]
public sealed partial class LoudspeakerHolderComponent : Component
{
    [DataField]
    public List<EntityUid> Loudspeakers = new();
}
