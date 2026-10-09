// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class HereticSacrificeConditionComponent : Component
{
    [DataField] public float Sacrificed = 0f;
    /// <summary>
    ///     Indicates that a victim should be a head role / command.
    /// </summary>
    [DataField] public bool IsCommand = false;
}
