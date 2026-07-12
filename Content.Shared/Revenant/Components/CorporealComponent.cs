// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// <Trauma>
using Robust.Shared.GameStates;
// </Trauma>

namespace Content.Shared.Revenant.Components;

/// <summary>
/// Makes the target solid, visible, and applies a slowdown.
/// Meant to be used in conjunction with statusEffectSystem
/// </summary>
[RegisterComponent] 
[NetworkedComponent, AutoGenerateComponentState] // Trauma
public sealed partial class CorporealComponent : Component
{
    /// <summary>
    /// The debuff applied when the component is present.
    /// </summary>
    [DataField, AutoNetworkedField] // Trauma - replaced ViewVariables
    public float MovementSpeedDebuff = 0.3f;
}
