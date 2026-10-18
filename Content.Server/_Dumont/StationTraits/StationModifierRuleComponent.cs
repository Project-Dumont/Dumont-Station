namespace Content.Server._Dumont.StationTraits;
[RegisterComponent, Access(typeof(StationModifierRuleSystem))]
public sealed partial class StationModifierRuleComponent : Component
{
    [DataField]
    public float LightEnergy = 1f;

    [DataField]
    public float LightRadius = 1f;

    [DataField]
    public float SolarPower = 1f;

    [DataField]
    public float ResearchPoints = 1f;
    [DataField]
    public bool HeatAlert;
}
