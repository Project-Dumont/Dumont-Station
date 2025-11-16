// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 JohnJohn <189290423+JohnJJohn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Execution;

/// <summary>
/// Used in any guns that shouldn't be able to be used for exucutions
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunExecutionBlacklistComponent : Component;
