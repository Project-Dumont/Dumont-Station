// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components;
// Dumont end

namespace Content.Trauma.Client.Heretic;

[RegisterComponent]
public sealed partial class ClothingToggleAnimationVisualsComponent : Component
{
    [DataField]
    public ToggleAnimationState State = ToggleAnimationState.All;
}
