using Content.Shared.Station.Components;
using Content.Shared.Station;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._DV.Medical.CrewMonitoring;

public sealed partial class LongRangeCrewMonitorSystem : EntitySystem
{
    [Dependency] private SharedStationSystem _station = default!;

    /// <summary>
    /// Finds an arbitrary station grid on the same map as the argument.
    /// Returns null if no grid was found.
    /// </summary>
    public EntityUid? FindStationGridInMap(MapId map)
    {
        var station = _station.GetStationInMap(map);
        return station is { } stationUid
            ? _station.GetLargestGrid(stationUid)
            : null;
    }
}
