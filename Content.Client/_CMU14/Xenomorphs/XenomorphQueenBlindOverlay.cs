using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphQueenBlindOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> ScreechBlindShader = "DumontXenomorphQueenBlind";

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;

    private readonly ShaderInstance _shader;

    public XenomorphQueenBlindOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index(ScreechBlindShader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out XenomorphQueenScreechBlindComponent? blind))
            return;

        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        var viewportSize = args.Viewport.Size;
        var worldHeight = args.WorldBounds.Box.Height;
        var pixelsPerTile = viewportSize.Y / worldHeight;

        var innerRadius = blind.Radius * pixelsPerTile;
        var outerRadius = (blind.Radius + 1f) * pixelsPerTile;

        var handle = args.WorldHandle;
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("innerRadius", (float)innerRadius);
        _shader.SetParameter("outerRadius", (float)outerRadius);

        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}

public sealed partial class XenomorphQueenBlindVisualSystem : Robust.Shared.GameObjects.EntitySystem
{
    [Dependency] private IOverlayManager _overlays = default!;
    public override void Initialize() => _overlays.AddOverlay(new XenomorphQueenBlindOverlay());
    public override void Shutdown() => _overlays.RemoveOverlay<XenomorphQueenBlindOverlay>();
}
