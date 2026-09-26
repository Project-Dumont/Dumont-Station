// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Robust.Shared.Map;

namespace Content.Trauma.Shared.Tackle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TacklingComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetCoordinates TackleStartPosition;

    [DataField, AutoNetworkedField]
    public TackleModifier? Source;
}
