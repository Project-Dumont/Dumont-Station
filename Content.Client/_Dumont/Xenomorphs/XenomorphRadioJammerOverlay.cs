// SPDX-License-Identifier: AGPL-3.0-or-later
// Adapted from Trauma Station's CurioShieldOverlay: https://github.com/Trauma-Station/Trauma-Station/pull/3546

using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Dumont.Xenomorphs;

public sealed partial class XenomorphRadioJammerOverlay : Overlay
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly SharedTransformSystem _transform;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public XenomorphRadioJammerOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = (int) DrawDepth.FloorEffects;
        _transform = _entities.System<SharedTransformSystem>();
        _shader = _prototypes.Index<ShaderPrototype>("DumontXenomorphRadioJammer").Instance().Duplicate();
        _shader.SetParameter("gridColor", new Color(0.76f, 0.24f, 1f, 1f));
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var query = _entities.EntityQueryEnumerator<XenomorphRadioJammerVisualComponent, TransformComponent>();
        handle.UseShader(_shader);
        while (query.MoveNext(out _, out var visual, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var scale = visual.ScaleAt(_timing.CurTime);
            if (scale <= 0f)
                continue;

            var position = _transform.GetWorldPosition(xform);
            if (!args.WorldAABB.Enlarged(visual.Radius).Contains(position))
                continue;

            var rotation = xform.GridUid is { } grid ? (float) _transform.GetWorldRotation(grid).Theta : 0f;
            handle.SetTransform(Matrix3x2.CreateRotation(rotation) * Matrix3x2.CreateTranslation(position));
            handle.DrawTextureRect(Texture.White, Box2.CenteredAround(Vector2.Zero, new Vector2(visual.Radius * scale * 2f)));
        }
        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}

public sealed partial class XenomorphRadioJammerVisualSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;

    public override void Initialize() => _overlays.AddOverlay(new XenomorphRadioJammerOverlay());
    public override void Shutdown() => _overlays.RemoveOverlay<XenomorphRadioJammerOverlay>();
}
