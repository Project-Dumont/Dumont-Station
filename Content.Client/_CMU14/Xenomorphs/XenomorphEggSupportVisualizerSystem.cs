using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphEggSupportVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphEggSupportComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<XenomorphEggSupportComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnStartup(Entity<XenomorphEggSupportComponent> ent, ref ComponentStartup args) => Apply(ent);
    private void OnState(Entity<XenomorphEggSupportComponent> ent, ref AfterAutoHandleStateEvent args) => Apply(ent);

    private void Apply(Entity<XenomorphEggSupportComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)
            || !_sprite.LayerMapTryGet((ent, sprite), "status", out var layer, false))
            return;
        var path = ent.Comp.Sustained ? "xeno_egg_fragile_eggsac" : ent.Comp.Fragile ? "xeno_egg_fragile" : "xeno_egg";
        _sprite.LayerSetRsi((ent, sprite), layer, new ResPath($"/Textures/_RMC14/Structures/Xenos/{path}.rsi"));
    }
}
