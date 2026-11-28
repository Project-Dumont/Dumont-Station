// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;

/// <summary>
/// Item that allows lock heretics to trap doors
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SerpentclaveComponent : Component
{
    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(1.5);
}
