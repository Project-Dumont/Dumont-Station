using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Utility;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphTailTetherSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;
    public override void Initialize() => _overlays.AddOverlay(new XenomorphTailTetherOverlay(EntityManager));
    public override void Shutdown() => _overlays.RemoveOverlay<XenomorphTailTetherOverlay>();
}

public sealed partial class XenomorphTailTetherOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;
    private readonly IEntityManager _entities;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprites;
    private readonly ContainerSystem _containers;
    private static readonly SpriteSpecifier.Rsi Texture = new(new ResPath("/Textures/_RMC14/Effects/beam.rsi"), "oppressor_tail");

    public XenomorphTailTetherOverlay(IEntityManager entities)
    {
        _entities = entities;
        _transform = entities.System<SharedTransformSystem>();
        _sprites = entities.System<SpriteSystem>();
        _containers = entities.System<ContainerSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var query = _entities.AllEntityQueryEnumerator<XenomorphTailTetherComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var tether, out var xform))
        {
            if (xform.MapID != args.MapId || !_entities.TryGetComponent<TransformComponent>(tether.Source, out var source)
                || source.MapID != args.MapId || _containers.IsEntityOrParentInContainer(uid, xform: xform)
                || _containers.IsEntityOrParentInContainer(tether.Source, xform: source)) continue;
            var start = _transform.GetWorldPosition(source);
            var end = _transform.GetWorldPosition(xform);
            if (!Box2.FromTwoPoints(start, end).Enlarged(0.5f).Intersects(args.WorldAABB)) continue;
            var difference = end - start;
            var center = start + difference / 2;
            var box = new Box2(-0.5f, -difference.Length() / 2, 0.5f, difference.Length() / 2);
            args.WorldHandle.DrawTextureRect(_sprites.GetState(Texture).Frame0, new Box2Rotated(box.Translated(center), difference.ToWorldAngle(), center));
        }
    }
}
