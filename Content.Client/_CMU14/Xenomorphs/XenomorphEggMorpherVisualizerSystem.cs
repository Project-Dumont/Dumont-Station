using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphEggMorpherVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphEggMorpherComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var morpher, out var sprite))
        {
            if (!_sprite.LayerMapTryGet((uid, sprite), EggmorpherOverlayLayers.Overlay, out var layer, false)) continue;
            _sprite.LayerSetVisible((uid, sprite), layer, morpher.CurParasites > 0);
            _sprite.LayerSetRsiState((uid, sprite), layer, $"eggmorph_{Math.Clamp((int)Math.Ceiling(morpher.CurParasites * 3f / morpher.MaxParasites), 0, 3)}");
        }
    }
}
