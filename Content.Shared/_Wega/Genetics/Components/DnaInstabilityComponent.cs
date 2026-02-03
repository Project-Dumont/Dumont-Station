namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class DnaInstabilityComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int Stage = 0;

    public float NextTimeTick;
}
