namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised before an emergency shuttle is sent to the station.
/// </summary>
[ByRefEvent]
public readonly record struct EmergencyShuttlePreDockEvent(EntityUid Shuttle);
