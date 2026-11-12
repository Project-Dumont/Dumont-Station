using Content.Shared.EntityEffects;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class StationSetWeather : EventEntityEffect<StationSetWeather>
{
    [DataField(required: true)]
    public ProtoId<WeatherPrototype> Weather;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
