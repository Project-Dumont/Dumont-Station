// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class EldritchSleepStatusEffectComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry ComponentsToAdd = new();

    [DataField]
    public ComponentRegistry ComponentDifference = new();
}
