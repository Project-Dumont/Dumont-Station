using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphCarrierVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private MobStateSystem _mobs = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphCarrierComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var carrier, out var sprite))
        {
            if (sprite.BaseRSI is not { } rsi)
                continue;
            var resting = TryComp<XenomorphRestComponent>(uid, out var rest) && rest.Resting;
            var downed = _mobs.IsCritical(uid);
            if (carrier.CanGenerateEggs)
            {
                if (!_sprite.LayerMapTryGet((uid, sprite), "eggsac", out var layer, false))
                    continue;
                _sprite.LayerSetVisible((uid, sprite), layer, !_mobs.IsDead(uid));
                var level = Math.Clamp((int)Math.Ceiling(carrier.Eggs * 3.0 / carrier.MaxEggs), 0, 3);
                var suffix = downed ? "_downed" : resting ? "_rest" : "";
                if (carrier.GeneratingEggs)
                    suffix += "_active";
                for (; level >= 0; level--)
                {
                    var state = $"eggsac_{level}{suffix}";
                    if (!rsi.TryGetState(state, out _))
                        continue;
                    _sprite.LayerSetRsiState((uid, sprite), layer, state);
                    break;
                }
                continue;
            }
            var visible = (int)Math.Ceiling(carrier.Parasites * 4.0 / carrier.MaxParasites);
            for (var i = 0; i < 4; i++)
            {
                if (!_sprite.LayerMapTryGet((uid, sprite), $"parasite{i}", out var layer, false))
                    continue;
                _sprite.LayerSetVisible((uid, sprite), layer, i < visible && !_mobs.IsDead(uid));
                var prefix = downed ? "para_downed_" : resting ? "para_rest_" : "para_";
                _sprite.LayerSetRsiState((uid, sprite), layer, prefix + i);
            }
        }
    }
}
