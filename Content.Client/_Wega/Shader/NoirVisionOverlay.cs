using Content.Shared.Shaders;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.Shaders.Systems;

public sealed class NoirVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private static readonly ProtoId<ShaderPrototype> Noir = "Noir";
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _redHighlightShader;

    public float RedThreshold = 0f;
    public float RedSaturation = 1.0f;

    public NoirVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _redHighlightShader = _prototypeManager.Index(Noir).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        if (_entityManager.TryGetComponent<NoirVisionComponent>(_playerManager.LocalEntity, out var noirVision))
        {
            RedThreshold = noirVision.RedThreshold;
            RedSaturation = noirVision.RedSaturation;
        }

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;

        _redHighlightShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _redHighlightShader.SetParameter("RedThreshold", RedThreshold);
        _redHighlightShader.SetParameter("RedSaturation", RedSaturation);
        handle.UseShader(_redHighlightShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
