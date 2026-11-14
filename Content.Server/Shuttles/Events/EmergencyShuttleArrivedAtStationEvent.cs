// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Shuttles.Events;

[ByRefEvent]
public readonly record struct EmergencyShuttleArrivedAtStationEvent(EntityUid Shuttle);
