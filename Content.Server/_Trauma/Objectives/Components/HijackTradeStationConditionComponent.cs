// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
///     Objective condition that requires the player to hijack the trade station.
/// </summary>
[RegisterComponent, Access(typeof(HijackTradeStationConditionSystem))]
public sealed partial class HijackTradeStationConditionComponent : Component
{
}
