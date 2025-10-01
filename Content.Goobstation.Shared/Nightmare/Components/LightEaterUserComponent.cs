// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Nightmare.Components;

/// <summary>
/// This is used for indicating that the user owns this action
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightEaterUserComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionLightEater";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId LightEaterProto = "LightEaterArmBlade";

    [DataField]
    public bool Activated;

    [DataField]
    public EntityUid? LightEaterEntity;
}
