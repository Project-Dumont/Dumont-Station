namespace Content.Server.Shuttles.Events;

[ByRefEvent]
public readonly record struct EmergencyShuttleArrivedAtStationEvent(EntityUid Shuttle);
