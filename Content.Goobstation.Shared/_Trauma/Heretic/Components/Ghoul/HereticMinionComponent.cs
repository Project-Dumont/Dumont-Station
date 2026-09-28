// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.StatusIcon;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Ghoul;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticMinionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? BoundHeretic;

    [DataField, AutoNetworkedField]
    public int MinionId;

    [DataField]
    public EntityUid? CreationRitual;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> MasterIcon { get; set; } = "GhoulHereticMaster";

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> GhoulIcon { get; set; } = "GhoulFaction";
}
