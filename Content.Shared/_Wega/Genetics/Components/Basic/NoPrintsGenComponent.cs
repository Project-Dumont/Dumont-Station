namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class NoPrintsGenComponent : Component
{
    [DataField]
    public string OldPrints = string.Empty;
}
