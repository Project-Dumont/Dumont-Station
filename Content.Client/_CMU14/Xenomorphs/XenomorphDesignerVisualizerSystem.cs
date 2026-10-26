using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Timing;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphDesignerVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprites = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        var xeno = HasComp<XenomorphComponent>(_player.LocalEntity);
        var nodes = EntityQueryEnumerator<XenomorphDesignNodeComponent, SpriteComponent>();
        while (nodes.MoveNext(out var uid, out _, out var sprite))
            _sprites.LayerSetVisible((uid, sprite), 1, xeno);
        var effects = EntityQueryEnumerator<XenomorphResinBuildVisualComponent, SpriteComponent>();
        while (effects.MoveNext(out var uid, out var effect, out var sprite))
        {
            if (!_sprites.TryGetLayer((uid, sprite), 1, out var layer, false) || layer.ActualState == null) continue;
            var progress = 1 - Math.Clamp((effect.End - _timing.CurTime).TotalSeconds / Math.Max(.001f, effect.Duration), 0, 1);
            var frame = (int)Math.Min(layer.ActualState.DelayCount * progress, layer.ActualState.DelayCount - 1);
            _sprites.LayerSetAutoAnimated(layer, layer.AnimationFrame < frame);
        }
    }
}
