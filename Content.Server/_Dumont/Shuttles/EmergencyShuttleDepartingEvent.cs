// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Dumont.Shuttles;

[ByRefEvent]
public readonly record struct EmergencyShuttleDepartingEvent(float StartupTime, float TransitTime);
