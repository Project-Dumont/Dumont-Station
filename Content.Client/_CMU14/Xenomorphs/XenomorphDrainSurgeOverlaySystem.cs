using Content.Client._Dumont.Xenomorphs;
using Robust.Client.Graphics;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphDrainSurgeOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        _overlay.AddOverlay(new XenomorphDrainSurgeOverlay(EntityManager));
    }

    public override void Shutdown()
    {
        _overlay.RemoveOverlay<XenomorphDrainSurgeOverlay>();
    }
}
