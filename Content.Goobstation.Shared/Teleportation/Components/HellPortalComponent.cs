// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 JohnJohn <189290423+JohnJJohn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Teleportation.Components;

/// <summary>
/// Component for Hell portal activation
/// </summary>
[RegisterComponent]
public sealed partial class HellPortalComponent : Component
{
    [DataField]
    public EntityUid? HellMap;

    [DataField]
    public EntityUid? ExitPortal;

    [DataField]
    public ResPath HellMapPath = new ResPath("/Maps/_Goobstation/Nonstations/Hell.yml");

    [DataField]
    public bool PortalEnabled;

    [DataField]
    public EntProtoId ExitPortalPrototype = "PortalHellExit";

}
