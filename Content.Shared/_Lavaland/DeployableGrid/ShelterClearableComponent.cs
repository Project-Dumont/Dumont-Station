namespace Content.Shared._Lavaland.DeployableGrid;

/// <summary>
/// Objects that do not block shelter deployment. Vegetation is cleared; ground decorations can be preserved.
/// </summary>
[RegisterComponent]
public sealed partial class ShelterClearableComponent : Component
{
    /// <summary>
    /// Preserve terrain decorations such as passable basalt instead of deleting them.
    /// </summary>
    [DataField]
    public bool RemoveOnDeploy = true;
}
