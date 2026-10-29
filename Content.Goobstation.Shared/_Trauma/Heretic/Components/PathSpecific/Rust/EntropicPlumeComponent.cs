// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;

[RegisterComponent, NetworkedComponent]
public sealed partial class EntropicPlumeComponent : Component
{
    [DataField]
    public float Duration = 10f;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new()
    {
        { "EldritchRust", 5f },
    };

    [DataField]
    public List<EntityUid> AffectedEntities = new();
}
