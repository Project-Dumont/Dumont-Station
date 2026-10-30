using Content.Shared._Lavaland.DeployableGrid;

namespace Content.Client._Lavaland.DeployableGrid;

using Content.Client.Examine;
using Content.Client.Hands.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;

public sealed class DeployableGridSystem : SharedDeployableGridSystem
{
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlays.AddOverlay(new ShelterDeploymentOverlay(this, EntityManager));
    }

    public override void Shutdown()
    {
        if (_overlays.TryGetOverlay<ShelterDeploymentOverlay>(out var overlay))
            overlay.Dispose();
        _overlays.RemoveOverlay<ShelterDeploymentOverlay>();
        base.Shutdown();
    }

    /// <summary>
    /// A preview is visible only while inspecting a capsule held by the local player.
    /// </summary>
    public bool TryGetPreview(out EntityUid capsule, out DeployableGridCheck check)
    {
        capsule = default;
        check = default;
        if (_examine.ExaminedEntity is not {} item
            || TerminatingOrDeleted(item)
            || _player.LocalEntity is not {} player
            || !_hands.IsHolding(player, item)
            || !TryComp<DeployableGridComponent>(item, out var component))
            return false;

        capsule = item;
        check = GetDeploymentCheck((item, component));
        return true;
    }

}
