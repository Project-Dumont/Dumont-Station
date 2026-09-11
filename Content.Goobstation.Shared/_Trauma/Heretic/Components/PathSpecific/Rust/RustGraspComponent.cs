// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;

[RegisterComponent]
public sealed partial class RustGraspComponent : Component
{
    [DataField]
    public float MinUseDelay = 0.7f;

    [DataField]
    public float MaxUseDelay = 3f;

    [DataField]
    public string Delay = "rust";

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
