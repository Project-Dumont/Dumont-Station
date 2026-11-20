// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.BloodSplatter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodSplatterOnLandComponent : Component
{
    [DataField]
    public EntProtoId Decal = "DecalSpawnerBloodSplattersTrauma";

    [DataField, AutoNetworkedField]
    public Color Color = Color.Red;

    [DataField]
    public bool DeleteEntity = true;
}
