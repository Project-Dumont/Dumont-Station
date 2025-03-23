using Content.Server.Atmos.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Genetics;

namespace Content.Server.Genetics.System;

public sealed class HeatResistanceGenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatResistanceGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HeatResistanceGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<HeatResistanceGenComponent> ent, ref ComponentInit args)
    {
        if (TryComp<FlammableComponent>(ent, out _))
        {
            RemComp<FlammableComponent>(ent);
            ent.Comp.RemFlammable = true;
        }

        if (TryComp<TemperatureComponent>(ent, out var temperature))
        {
            temperature.HeatDamageThreshold = temperature.HeatDamageThreshold * ent.Comp.ResistanceRatio;
        }
    }

    private void OnShutdown(Entity<HeatResistanceGenComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.RemFlammable) AddComp<FlammableComponent>(ent);

        if (TryComp<TemperatureComponent>(ent, out var temperature))
        {
            temperature.HeatDamageThreshold = temperature.HeatDamageThreshold / ent.Comp.ResistanceRatio;
        }
    }
}

