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

[RegisterComponent, NetworkedComponent]
public sealed partial class StarBlastComponent : Component
{
    [DataField]
    public float StarMarkRadius = 3f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntityUid Action;
}
