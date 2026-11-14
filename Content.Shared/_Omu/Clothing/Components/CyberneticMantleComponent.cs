// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Shared._Omu.Clothing.EntitySystems;
using Content.Shared._Omu.Traits;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Omu.Clothing.Components;

/// <summary>
///     This component indicates that an item is a Cybernetic Beast's mantle.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(CyberneticMantleSystem), typeof(CyberneticBeastSystem))]
public sealed partial class CyberneticMantleComponent : Component, IClothingSlots
{

    /// <summary>
    ///     The mantle will relay an event to these inventory slots when equpped.
    /// </summary>
    [DataField]
    public SlotFlags Slots { get; set; } = SlotFlags.MASK;

    /// <summary>
    ///     The minimum "value" (brightness) of the eyes on the visor.
    ///     In effect, if the player's eyes have an HSV value where V is less than this value,
    ///     the value on the visor will be raised be at least this value.
    /// </summary>
    [DataField]
    public float MinEyeColorLevel { get; set; } = 0.5f;

    [DataField, AutoNetworkedField]
    public EntityUid? LightToggleActionEntity;
}

[Serializable, NetSerializable]
public enum CyberneticMantleVisuals : byte
{
    EyeColor,
}
