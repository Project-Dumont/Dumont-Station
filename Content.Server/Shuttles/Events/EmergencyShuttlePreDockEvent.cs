// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised before an emergency shuttle is sent to the station.
/// </summary>
[ByRefEvent]
public readonly record struct EmergencyShuttlePreDockEvent(EntityUid Shuttle);
