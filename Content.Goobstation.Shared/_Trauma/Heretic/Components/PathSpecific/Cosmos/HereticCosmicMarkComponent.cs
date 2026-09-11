// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticCosmicMarkComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? CosmicDiamondUid;

    [DataField, AutoNetworkedField]
    public int PassiveLevel;

    [DataField]
    public EntProtoId CosmicDiamond = "EffectCosmicDiamond";

    [DataField]
    public EntProtoId CosmicCloud = "EffectCosmicCloud";
}
