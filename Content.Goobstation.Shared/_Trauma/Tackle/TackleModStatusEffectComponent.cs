// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Tackle;

/// <summary>
/// Status effect component that modifies tackle effectiveness.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class TackleModStatusEffectComponent : Component
{
    /// <summary>
    /// Value to add to the tackle modifier event's Modifier field.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float Modifier;
}
