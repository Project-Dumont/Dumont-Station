// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.DeviceLinking;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveDeviceLinkSinkComponent : Component
{
    /// <summary>
    /// Counts the amount of times a sink has been invoked for severing the link if this counter gets to high
    /// The counter is counted down by one every tick if it's higher than 0
    /// This is for preventing infinite loops
    /// </summary>
    [DataField]
    public int InvokeCounter;
}
