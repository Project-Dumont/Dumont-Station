using Content.Server.Station.Systems;
using Content.Shared._Dumont.EntityEffects;
using Content.Shared.EntityEffects;
using Content.Shared.Weather;

namespace Content.Server._Dumont.Weather;

public sealed partial class StationSetWeatherSystem : EntitySystem
{
    [Dependency] private SharedWeatherSystem _weather = default!;
    [Dependency] private StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationSetWeather>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationSetWeather> args)
    {
        if (_station.GetLargestGrid(args.Args.TargetEntity) is not { } grid)
            return;

        _weather.SetWeather(Transform(grid).MapID, ProtoMan.Index(args.Effect.Weather), null);
    }
}
