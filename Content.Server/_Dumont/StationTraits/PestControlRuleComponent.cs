namespace Content.Server._Dumont.StationTraits;
[RegisterComponent]
public sealed partial class PestControlRuleComponent : Component
{
    [DataField]
    public float SpawnChance = 0.25f;
}
