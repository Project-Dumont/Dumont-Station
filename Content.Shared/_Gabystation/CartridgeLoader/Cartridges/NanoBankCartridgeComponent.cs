// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.NanoBank;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[RegisterComponent, NetworkedComponent]
public sealed partial class NanoBankCartridgeComponent : Component
{
    /// <summary>
    ///     The NanoBank card.
    /// </summary>
    [DataField]
    public EntityUid? Card;
}
