using Content.Shared.Atmos;
using Content.Server._Gabystation.Mech.Equipment.EntitySystems;

namespace Content.Server._Gabystation.Mech.Equipment.Components;

[RegisterComponent, Access(typeof(MechAirRecyclerSystem))]
public sealed partial class MechAirRecyclerComponent : Component
{
    [DataField("enabled")]
    public bool Enabled = true;

    [DataField("energyCost")]
    public float EnergyCost = 0.5f;

    [DataField("targetPressure")]
    public float TargetPressure = Atmospherics.OneAtmosphere;

    [DataField("targetTemperature")]
    public float TargetTemperature = Atmospherics.T20C;

    [DataField("oxygenRatio")]
    public float OxygenRatio = 0.22f;

    [DataField("nitrogenRatio")]
    public float NitrogenRatio = 0.78f;
}
