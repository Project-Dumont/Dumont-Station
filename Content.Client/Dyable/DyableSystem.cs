using Content.Shared.Dyable;
using Robust.Client.GameObjects;

namespace Content.Client.Dyable;

public sealed partial class DyableSystem : SharedDyableSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DyableComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<DyableComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not {} sprite)
            return;

        if (!Appearance.TryGetData<Color>(ent.Owner, DyableVisuals.Color, out var color, args.Component))
            return;

        if (!sprite.LayerMapTryGet(DyableVisualsLayers.Layer, out var layer))
            return;

        sprite.LayerSetColor(layer, color);
    }
}
