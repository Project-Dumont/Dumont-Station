// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.GoliathTentacle;

/// <summary>
/// Component that grants the entity the ability to use goliath tentacles.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GoliathTentacleComponent : Component
{
    [DataField]
    public EntProtoId? Action = "ActionGoliathTentacleCrew";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}