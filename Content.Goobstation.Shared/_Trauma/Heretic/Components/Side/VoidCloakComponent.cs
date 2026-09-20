// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoidCloakComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Transparent;
}

[Serializable, NetSerializable]
public enum VoidCloakVisuals : byte
{
    Transparent,
}
