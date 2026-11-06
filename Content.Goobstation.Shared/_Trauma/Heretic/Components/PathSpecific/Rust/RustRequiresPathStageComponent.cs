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
public sealed partial class RustRequiresPathStageComponent : Component
{
    /// <summary>
    /// If rust heretic path stage is less than this - they won't be able to rust this surface
    /// </summary>
    [DataField]
    public int PathStage = 2;
}
