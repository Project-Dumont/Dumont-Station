using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphDespoilerShotVisualSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphDespoilerShotVisualComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<XenomorphDespoilerShotVisualComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnStartup(EntityUid uid, XenomorphDespoilerShotVisualComponent comp, ComponentStartup args)
    {
        Apply(uid, comp);
    }

    private void OnState(EntityUid uid, XenomorphDespoilerShotVisualComponent comp, ref AfterAutoHandleStateEvent args)
    {
        Apply(uid, comp);
    }

    private void Apply(EntityUid uid, XenomorphDespoilerShotVisualComponent comp)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        _sprite.SetScale((uid, sprite), comp.Scale);
    }
}
