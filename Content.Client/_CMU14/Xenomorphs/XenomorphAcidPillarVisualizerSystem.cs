using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphAcidPillarVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphAcidPillarComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var pillar, out var sprite))
            _sprite.LayerSetRsiState((uid, sprite), 0, _timing.CurTime < pillar.FiringUntil ? "attack" : "idle");
    }
}
