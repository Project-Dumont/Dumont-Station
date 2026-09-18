// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Body;

/// <summary>
/// Body part that adds to base movement speed values while attached.
/// Not required for movement.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeedModifierOrganComponent : Component
{
    [DataField]
    public float WeightlessFriction;

    [DataField]
    public float WeightlessModifier;

    [DataField]
    public float WeightlessAcceleration;

    // add more if you want to use them :)
}

/// <summary>
/// Added to a body that has had a <see cref="SpeedModifierOrganComponent"/> part attached,
/// so its weightless movement gets refreshed with the part bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeedModifierOrganBodyComponent : Component;
