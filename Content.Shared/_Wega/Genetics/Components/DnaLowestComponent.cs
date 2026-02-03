namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class DnaLowestComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Parent = default!;
}
