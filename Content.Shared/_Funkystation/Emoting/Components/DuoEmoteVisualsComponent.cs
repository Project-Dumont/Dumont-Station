// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Emoting.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Emoting.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class DuoEmoteVisualsComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<DuoEmotePrototype> EmoteId;
}
