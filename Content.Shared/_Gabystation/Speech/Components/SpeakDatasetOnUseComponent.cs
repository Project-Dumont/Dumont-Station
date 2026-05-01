// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.Speech.EntitySystems;
using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.Speech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SpeakDatasetOnUseSystem))]
public sealed partial class SpeakDatasetOnUseComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<LocalizedDatasetPrototype> Dataset;
}
