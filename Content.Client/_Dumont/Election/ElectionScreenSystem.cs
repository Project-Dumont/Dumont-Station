using Content.Shared._Dumont.Election;
using Robust.Client.Graphics;

namespace Content.Client._Dumont.Election;

public sealed partial class ElectionScreenSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    private ElectionScreenOverlay _screen = default!;

    public override void Initialize()
    {
        base.Initialize();

        _screen = new ElectionScreenOverlay();
        _overlay.AddOverlay(_screen);

        SubscribeNetworkEvent<ElectionStateEvent>(OnState);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _screen.SetState(null);
        _overlay.RemoveOverlay(_screen);
    }

    private void OnState(ElectionStateEvent ev)
    {
        _screen.SetState(ev);
    }
}
