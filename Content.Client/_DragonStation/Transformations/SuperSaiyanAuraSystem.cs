using Content.Shared._DragonStation.Transformations;
using Robust.Client.GameObjects;

namespace Content.Client._DragonStation.Transformations;

public sealed class SuperSaiyanAuraSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SuperSaiyanAuraComponent, ComponentStartup>(OnAuraAdded);
        SubscribeLocalEvent<SuperSaiyanAuraComponent, ComponentShutdown>(OnAuraRemoved);
    }

    private void OnAuraAdded(Entity<SuperSaiyanAuraComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.Sprite == null)
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite) ||
            sprite.LayerMapTryGet(SuperSaiyanAuraVisualKey.Key, out _))
        {
            return;
        }

        var layer = sprite.AddLayer(ent.Comp.Sprite, 0);
        sprite.LayerMapSet(SuperSaiyanAuraVisualKey.Key, layer);
        sprite.LayerSetOffset(layer, ent.Comp.Offset);
        sprite.LayerSetScale(layer, ent.Comp.Scale);
        sprite.LayerSetShader(layer, ent.Comp.Shader);
    }

    private void OnAuraRemoved(Entity<SuperSaiyanAuraComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.RemoveLayer(SuperSaiyanAuraVisualKey.Key);
    }
}
