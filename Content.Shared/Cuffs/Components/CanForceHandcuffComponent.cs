// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Shared.Cuffs.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedCuffableSystem))]
public sealed partial class CanForceHandcuffComponent : Component
{
    public const string ContainerId = "force-handcuff";

    [DataField]
    public EntProtoId HandcuffsId = "Handcuffs";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Handcuffs;

    [ViewVariables, AutoNetworkedField]
    public BaseContainer? Container;

    [DataField]
    public bool RequireHands = true;

    [DataField]
    public bool Complex = false;
}
