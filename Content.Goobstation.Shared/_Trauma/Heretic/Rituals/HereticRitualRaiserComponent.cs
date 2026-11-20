// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticRitualRaiserComponent : Component
{
    /// <summary>
    /// Used for events to heretic ritual events to store their results for other methods to use
    /// </summary>
    [DataField, NonSerialized]
    public Dictionary<string, object> Blackboard = new();

    public HereticRitualRaiser Raiser = default!;
}
