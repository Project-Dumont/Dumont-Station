// SPDX-License-Identifier: AGPL-3.0-or-later


// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// Grants you shadow grapple action
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowGrappleComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ShadowGrappleAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionUid;
}
