using System.Numerics;
using Content.Shared._Lavaland.DeployableGrid;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.Client._Lavaland.DeployableGrid;

/// <summary>
/// Shows the shelter footprint and its current blockers only during inspection.
/// </summary>
public sealed class ShelterDeploymentOverlay(DeployableGridSystem system, IEntityManager entities) : Overlay
{
    private readonly ShelterGhostRenderer _ghosts = new(entities);

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    protected override void DisposeBehavior()
    {
        _ghosts.Dispose();
        base.DisposeBehavior();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!system.TryGetPreview(out var capsule, out var check))
            return;
        var transform = entities.System<SharedTransformSystem>();
        if (entities.GetComponent<TransformComponent>(capsule).MapID != args.MapId)
            return;

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        var color = check.CanDeploy ? Color.LimeGreen : Color.Orange;
        if (entities.GetComponent<DeployableGridComponent>(capsule).Ghost is {} ghost)
        {
            var texture = _ghosts.GetTexture(ghost, args.RenderHandle);
            handle.DrawTextureRect(texture, check.Bounds.Enlarged(1f), Color.White.WithAlpha(0.5f));
        }
        else
            handle.DrawRect(check.Bounds, color.WithAlpha(0.12f));
        handle.DrawRect(check.Bounds, check.CanDeploy ? Color.LimeGreen : Color.Red, filled: false);
        if (!check.OnPlanet)
        {
            handle.DrawLine(check.Bounds.BottomLeft, check.Bounds.TopRight, Color.Red);
            handle.DrawLine(check.Bounds.TopLeft, check.Bounds.BottomRight, Color.Red);
        }

        foreach (var blocker in check.BlockingEntities)
        {
            if (!entities.TryGetComponent<TransformComponent>(blocker, out var xform))
                continue;
            var tile = transform.GetWorldPosition(xform).Floored();
            var bounds = new Box2((Vector2) tile, (Vector2) tile + Vector2.One);
            handle.DrawRect(bounds, Color.Red.WithAlpha(0.35f));
            handle.DrawRect(bounds, Color.Red, filled: false);
            handle.DrawLine(bounds.BottomLeft, bounds.TopRight, Color.Red);
            handle.DrawLine(bounds.TopLeft, bounds.BottomRight, Color.Red);
        }
        foreach (var grid in check.NearbyGrids)
        {
            if (!entities.TryGetComponent<MapGridComponent>(grid, out var component))
                continue;
            var bounds = transform.GetWorldMatrix(grid).TransformBox(component.LocalAABB);
            handle.DrawRect(bounds, Color.Red.WithAlpha(0.15f));
            handle.DrawRect(bounds, Color.Red, filled: false);
        }
    }
}
