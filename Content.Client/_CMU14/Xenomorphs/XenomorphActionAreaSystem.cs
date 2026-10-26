using Content.Client._Dumont.Xenomorphs;
using Robust.Client.Graphics;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphActionAreaSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlays.AddOverlay(new XenomorphActionAreaOverlay());
    }

    public override void Shutdown()
    {
        _overlays.RemoveOverlay<XenomorphActionAreaOverlay>();
        base.Shutdown();
    }
}
