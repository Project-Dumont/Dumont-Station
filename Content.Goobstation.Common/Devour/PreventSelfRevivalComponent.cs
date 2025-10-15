// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Devour;

/// <summary>
/// Used to mark an entity as being unable to self-revive (e.g preventing Changelings from using their stasis)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventSelfRevivalComponent : Component;
