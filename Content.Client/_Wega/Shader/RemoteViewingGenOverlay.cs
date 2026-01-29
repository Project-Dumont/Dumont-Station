// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Genetics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Genetics;

public sealed class RemoteViewingGenOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly TransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly ContainerSystem _container;

    public override bool RequestScreenTexture => false;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public RemoteViewingGenOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _sprite = _entity.System<SpriteSystem>();
        ZIndex = 100; 
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var worldHandle = args.WorldHandle;
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var player = _player.LocalEntity;
        if (player == null)
            return;

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;
        var query = _entity.EntityQueryEnumerator<RemoteViewingGenComponent, SpriteComponent, TransformComponent>();
        
        while (query.MoveNext(out var uid, out var genComp, out var sprite, out var xform))
        {
            if (uid == player)
                continue;

            if (xform.MapID != mapId)
                continue;

            if (!sprite.Visible)
                continue;

            var entityToRender = uid;
            var spriteToRender = sprite;
            var xformToRender = xform;

            if (_container.TryGetOuterContainer(uid, xform, out var container))
            {
                var owner = container.Owner;
                if (_entity.TryGetComponent<SpriteComponent>(owner, out var ownerSprite)
                    && _entity.TryGetComponent<TransformComponent>(owner, out var ownerXform))
                {
                    entityToRender = owner;
                    spriteToRender = ownerSprite;
                    xformToRender = ownerXform;
                }
            }

            var position = _transform.GetWorldPosition(xformToRender);
            var rotation = _transform.GetWorldRotation(xformToRender);

            _sprite.RenderSprite((entityToRender, spriteToRender), worldHandle, eyeRot, rotation, position);
        }

        worldHandle.SetTransform(Matrix3x2.Identity);
    }
}
