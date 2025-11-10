// SPDX-FileCopyrightText: 2025 Crono209ggg <crono209gg@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.SurgeryBlocker;

/// <summary>
/// Impede que alguem tente usar cirurgia em alguma parte espescifica 
/// Usei no aracne porque o EE é maluco
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBlockerComponent : Component
{
}
