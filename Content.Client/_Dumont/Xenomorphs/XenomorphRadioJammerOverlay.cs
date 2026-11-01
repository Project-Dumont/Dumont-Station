// SPDX-License-Identifier: AGPL-3.0-or-later
// Adapted from Trauma Station's CurioShieldOverlay: https://github.com/Trauma-Station/Trauma-Station/pull/3546

using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Dumont.Xenomorphs;

public sealed partial class XenomorphRadioJammerOverlay : Overlay
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    private readonly SharedTransformSystem _transform;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public XenomorphRadioJammerOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = (int) DrawDepth.FloorEffects;
        _transform = _entities.System<SharedTransformSystem>();
        _shader = _prototypes.Index<ShaderPrototype>("DumontXenomorphRadioJammer").Instance().Duplicate();
        _shader.SetParameter("gridColor", new Color(0.71f, 0.43f, 0.84f, 0.45f));
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var query = _entities.EntityQueryEnumerator<XenomorphRadioJammerVisualComponent, TransformComponent>();
        handle.UseShader(_shader);
        while (query.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var position = _transform.GetWorldPosition(xform);
            if (!args.WorldAABB.Enlarged(2f).Contains(position))
                continue;

            handle.DrawTextureRect(Texture.White, Box2.CenteredAround(position, new Vector2(3.6f)));
        }
        handle.UseShader(null);
    }
}

public sealed partial class XenomorphRadioJammerVisualSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;

    public override void Initialize() => _overlays.AddOverlay(new XenomorphRadioJammerOverlay());
    public override void Shutdown() => _overlays.RemoveOverlay<XenomorphRadioJammerOverlay>();
}
