// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Sprite;
using Robust.Client.GameObjects;

namespace Content.Client.Sprite;

public sealed class SimpleSpriteOverlaySystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SimpleSpriteOverlayComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<SimpleSpriteOverlayComponent, ComponentShutdown>(OnCompShutdown);
    }

    private void OnAfterHandleState(Entity<SimpleSpriteOverlayComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var index = _sprite.LayerMapReserve((ent.Owner, sprite), ent.Comp.LayerMap);

        _sprite.LayerSetSprite((ent.Owner, sprite), index, ent.Comp.OverlaySprite);
        _sprite.LayerSetVisible((ent.Owner, sprite), index, true);

        if (ent.Comp.Shader is not null)
            sprite.LayerSetShader(index, ent.Comp.Shader);
    }

    private void OnCompShutdown(Entity<SimpleSpriteOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((ent.Owner, sprite), ent.Comp.LayerMap, out var index, true))
            _sprite.LayerSetVisible((ent.Owner, sprite), index, false);
    }
}
