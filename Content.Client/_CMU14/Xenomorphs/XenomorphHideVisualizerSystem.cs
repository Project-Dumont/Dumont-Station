using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphHideVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprites = default!;
    [Dependency] private MobStateSystem _mobs = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphHideComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var hide, out var sprite))
        {
            _sprites.SetDrawDepth((uid, sprite), (int)(hide.Hiding && _mobs.IsAlive(uid) ? DrawDepth.Walls
                : _mobs.IsDead(uid) ? DrawDepth.DeadMobs : DrawDepth.Mobs));
        }
    }
}
