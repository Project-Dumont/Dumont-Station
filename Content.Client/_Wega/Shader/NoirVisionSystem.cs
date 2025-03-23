using Content.Client.Shaders.Systems;
using Content.Shared.Genetics;
using Content.Shared.Shaders;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Shaders.System;

public sealed class NoirVisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private NoirVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoirVisionComponent, ComponentInit>(OnDizzyInit);
        SubscribeLocalEvent<NoirVisionComponent, ComponentShutdown>(OnDizzyShutdown);

        SubscribeLocalEvent<NoirVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NoirVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, NoirVisionComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, NoirVisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnDizzyInit(EntityUid uid, NoirVisionComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
        {
            _overlay.RedThreshold = component.RedThreshold;
            _overlay.RedSaturation = component.RedSaturation;
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void OnDizzyShutdown(EntityUid uid, NoirVisionComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.RemoveOverlay(_overlay);
    }
}
