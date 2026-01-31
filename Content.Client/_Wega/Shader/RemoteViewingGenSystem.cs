// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Overlays;
using Content.Shared.Genetics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Genetics;

public sealed class RemoteViewingGenSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private RemoteViewingGenOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new RemoteViewingGenOverlay();

        SubscribeLocalEvent<RemoteViewingGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RemoteViewingGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RemoteViewingGenComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<RemoteViewingGenComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnInit(Entity<RemoteViewingGenComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity == ent.Owner)
        {
            EnableOverlay();
        }
    }

    private void OnShutdown(Entity<RemoteViewingGenComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
        {
            DisableOverlay();
        }
    }

    private void OnPlayerAttached(Entity<RemoteViewingGenComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        EnableOverlay();
    }

    private void OnPlayerDetached(Entity<RemoteViewingGenComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        DisableOverlay();
    }

    private void EnableOverlay()
    {
        if (!_overlayMan.HasOverlay<RemoteViewingGenOverlay>())
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    private void DisableOverlay()
    {
        if (_overlayMan.HasOverlay<RemoteViewingGenOverlay>())
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
