using Content.Server.Atmos.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Atmos;
using Content.Shared.Genetics;

namespace Content.Server.Genetics.System;

public sealed class ColdResistanceGenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ColdResistanceGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ColdResistanceGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<ColdResistanceGenComponent> ent, ref ComponentInit args)
    {
        if (TryComp<TemperatureComponent>(ent, out var temperature))
        {
            ent.Comp.OldColdResistance = temperature.ColdDamageThreshold;
            temperature.ColdDamageThreshold = Atmospherics.TCMB;
        }

        if (HasComp<BarotraumaComponent>(ent))
            EnsureComp<PressureImmunityComponent>(ent);
    }

    private void OnShutdown(Entity<ColdResistanceGenComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<TemperatureComponent>(ent, out var temperature))
            temperature.ColdDamageThreshold = ent.Comp.OldColdResistance;

        if (HasComp<BarotraumaComponent>(ent) && HasComp<PressureImmunityComponent>(ent))
            RemComp<PressureImmunityComponent>(ent);
    }
}

