// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.SprayPainter.Components;

/// <summary>
/// Items with this component can be used to recharge a spray painter.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SprayPainterAmmoSystem))]
public sealed partial class SprayPainterAmmoComponent : Component
{
    /// <summary>
    /// The value by which the charge in the spray painter will be recharged.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Charges = 15;
}
