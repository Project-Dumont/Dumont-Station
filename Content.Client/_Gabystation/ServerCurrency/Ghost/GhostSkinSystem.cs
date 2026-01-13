// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.ServerCurrency.Ghost;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._Gabystation.ServerCurrency.Ghost;

public sealed class GhostSkinSystem : SharedGhostSkinSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private ProtoId<GhostSkinListingPrototype> _defaultSkin = "DefaultSkin";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostSkinComponent, AfterAutoHandleStateEvent>(OnStateChange);
    }

    private void OnStateChange(Entity<GhostSkinComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateSkin(ent);
    }

    private void UpdateSkin(Entity<GhostSkinComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite))
            return;

        if (ent.Comp.Skin is null)
        {
            //ResetSkin(ent, sprite);
            if (!_proto.TryIndex(_defaultSkin, out var proto))
                return;

            SetSkinFromProto(ent, sprite, proto);
        }

        else
        {
            if (!_proto.TryIndex(ent.Comp.Skin, out var proto))
                return;

            SetSkinFromProto(ent, sprite, proto);
        }

        _sprite.SetDrawDepth((ent.Owner, sprite), DrawDepth.Default + 11);
        sprite.OverrideContainerOcclusion = true;
        sprite.NoRotation = true;
    }

    private void SetSkinFromProto(Entity<GhostSkinComponent> ent, SpriteComponent sprite, GhostSkinListingPrototype? proto)
    {
        if (proto is null)
            return;

        var sprEnt = (ent.Owner, sprite);
        if (!_sprite.LayerMapTryGet(sprEnt, EffectLayers.Unshaded, out var layer, false))
            layer = 0;

        _sprite.LayerSetSprite(sprEnt, layer, proto.Sprite);
        sprite.LayerSetShader(layer, "unshaded");
        _sprite.LayerSetColor(sprEnt, layer, Color.TryFromHex(proto.Color) ?? Color.White);
        _sprite.LayerSetScale(sprEnt, layer, proto.Scale);
    }

    //! I have no idea how to reset the layer based on the prototype
    /* private void ResetSkin(Entity<GhostSkinComponent> ent, SpriteComponent sprite)
    {
        var proto = Prototype(ent.Owner);
        if (proto is null)
            return;

        if (!proto.TryGetComponent<SpriteComponent>(out var protoSprite))
            return;

        var sprEnt = (ent.Owner, sprite);
        if (!_sprite.LayerMapTryGet(sprEnt, EffectLayers.Unshaded, out var layer, false))
            return;

        if (!protoSprite.LayerMapTryGet(EffectLayers.Unshaded, out var protoLayer))
            return;
    } */
}
