// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Power.Components;

/// <summary>
/// Used to charge a battery with <see cref="PowerConsumerComponent"/>
/// instead of <see cref="BatteryChargerComponent"/> and <see cref="PowerNetworkBatteryComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class PowerConsumerBatteryChargerComponent : Component
{
    /// <summary>
    /// How much of the power consumed can be used to charge the battery
    /// </summary>
    [DataField]
    public float Efficiency = 1f;
}
