// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Harmony.Conspirators.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConspiratorComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> ConspiratorIcon = "ConspiratorFaction";

    public override bool SessionSpecific => true;
}
