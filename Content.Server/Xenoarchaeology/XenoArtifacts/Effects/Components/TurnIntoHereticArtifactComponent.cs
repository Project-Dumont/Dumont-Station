namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that turns person into heretic.
/// </summary>
[RegisterComponent]
public sealed partial class TurnIntoHereticArtifactComponent : Component
{
    [DataField]
    public string? Rule;
}
