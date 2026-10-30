namespace Content.Shared._Lavaland.DeployableGrid;

/// <summary>
/// The same placement result used by deployment and the inspection preview.
/// </summary>
public readonly record struct DeployableGridCheck(
    Box2 Bounds,
    bool OnPlanet,
    IReadOnlyList<EntityUid> BlockingEntities,
    IReadOnlyList<EntityUid> NearbyGrids)
{
    public bool CanDeploy => FailureMessage == null;

    public string? FailureMessage => !OnPlanet ? "shelter-capsule-fail-no-planet"
        : NearbyGrids.Count > 0 ? "shelter-capsule-fail-near-grid"
        : BlockingEntities.Count > 0 ? "shelter-capsule-fail-no-space"
        : null;
}
