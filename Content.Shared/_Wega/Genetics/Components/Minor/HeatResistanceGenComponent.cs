namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class HeatResistanceGenComponent : Component
{
    [DataField]
    public float ResistanceRatio = 1.5f;

    public bool RemFlammable = false;
}
