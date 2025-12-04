using Content.Shared.Genetics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Genetics.System;

public sealed class DizzyOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private static readonly ProtoId<ShaderPrototype> Dizzy = "Dizzy";
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _dizzyShader;

    public float CurrentIntensity = 0.0f;

    private const float VisualThreshold = 5.0f;
    private const float IntensityDivisor = 100.0f;

    private float _visualScale = 0;

    public DizzyOverlay()
    {
        IoCManager.InjectDependencies(this);
        _dizzyShader = _prototypeManager.Index(Dizzy).InstanceUnique();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;

        if (playerEntity == null)
            return;

        if (!_entityManager.TryGetComponent<DizzyEffectComponent>(playerEntity, out var dizzyEffect))
            return;

        CurrentIntensity = dizzyEffect.Intensity;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        _visualScale = IntensityToVisual(CurrentIntensity);
        return _visualScale > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _dizzyShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _dizzyShader.SetParameter("intensity", _visualScale);
        handle.UseShader(_dizzyShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private float IntensityToVisual(float intensity)
    {
        if (intensity < VisualThreshold)
            return 0;

        return Math.Clamp((intensity - VisualThreshold) / (IntensityDivisor / 2), 0.0f, 1.0f);
    }
}
