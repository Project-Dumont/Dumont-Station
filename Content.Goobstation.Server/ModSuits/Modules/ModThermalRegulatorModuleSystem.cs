using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;

namespace Content.Goobstation.Server.ModSuits.Modules;

public sealed class ModThermalRegulatorModuleSystem : EntitySystem
{
    [Dependency] private TemperatureSystem _temperature = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ModThermalRegulatorModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out _, out var regulator, out var module))
        {
            if (!module.Active
                || !TryComp<ModSuitComponent>(module.Suit, out var suit)
                || suit.Wearer is not { } wearer
                || !TryComp<TemperatureComponent>(wearer, out var temperature))
                continue;

            var difference = regulator.TargetTemperature - temperature.CurrentTemperature;
            if (MathF.Abs(difference) < 0.01f)
                continue;

            var step = MathF.Sign(difference) * MathF.Min(MathF.Abs(difference), regulator.Rate * frameTime);
            var heat = step * _temperature.GetHeatCapacity(wearer, temperature);
            _temperature.ChangeHeat(wearer, heat, true, temperature);
        }
    }
}
