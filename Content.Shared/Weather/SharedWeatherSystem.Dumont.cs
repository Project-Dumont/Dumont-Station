// Dumont start
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared.Weather;

public abstract partial class SharedWeatherSystem
{
    public bool TryAddWeather(MapId mapId, ProtoId<WeatherPrototype> prototype)
    {
        if (!_mapSystem.TryGetMap(mapId, out var mapUid))
            return false;

        var component = EnsureComp<WeatherComponent>(mapUid.Value);
        if (component.Weather.TryGetValue(prototype, out var existing))
        {
            existing.EndTime = null;
            if (existing.State == WeatherState.Ending)
                existing.State = WeatherState.Running;
            Dirty(mapUid.Value, component);
        }
        else
            StartWeather(mapUid.Value, component, ProtoMan.Index(prototype), null);
        return true;
    }

    public bool TryRemoveWeather(EntityUid map, ProtoId<WeatherPrototype> prototype)
    {
        if (!TryComp<WeatherComponent>(map, out var component) || !component.Weather.ContainsKey(prototype))
            return false;
        EndWeather(map, component, prototype);
        return true;
    }
}
// Dumont end
