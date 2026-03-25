using System.Numerics;
using Content.Client.Resources;
using Content.Shared._DragonStation.PowerLevel;
using Content.Shared.Damage;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Client.Overlays;

/// <summary>
/// Draws a tiny medhud-style power level text above eligible entities.
/// </summary>
public sealed class EntityPowerLevelOverlay : Overlay
{
    private const string TextFontPath = "/Fonts/NotoSans/NotoSans-Bold.ttf";
    private const int TextFontSize = 11;

    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _spriteSystem;
    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;
    public HashSet<string> DamageContainers = new();

    public EntityPowerLevelOverlay(IEntityManager entManager, IResourceCache resourceCache)
    {
        _entManager = entManager;
        _transform = _entManager.System<SharedTransformSystem>();
        _spriteSystem = _entManager.System<SpriteSystem>();
        _font = resourceCache.GetFont(TextFontPath, TextFontSize);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;

        args.DrawingHandle.SetTransform(Matrix3x2.Identity);

        var bounds = args.WorldBounds.Enlarged(1.5f);
        var query = _entManager.EntityQueryEnumerator<PowerLevelComponent, DamageableComponent, SpriteComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var powerLevel, out var damageable, out var sprite, out var transform))
        {
            if (!sprite.Visible || transform.MapID != args.MapId)
                continue;

            if (damageable.DamageContainerID == null || !DamageContainers.Contains(damageable.DamageContainerID))
                continue;

            var worldPos = _transform.GetWorldPosition(transform);
            if (!bounds.Contains(worldPos))
                continue;

            var text = $"PL {powerLevel.CurrentPowerLevel}";
            var localBounds = _spriteSystem.GetLocalBounds((uid, sprite));
            var textSize = args.ScreenHandle.GetDimensions(_font, text, 1f);
            var widthOfMob = localBounds.Width * EyeManager.PixelsPerMeter;
            var yOffset = localBounds.Height * EyeManager.PixelsPerMeter / 2f - 3f;
            var anchorWorld = worldPos + new Vector2(
                widthOfMob / EyeManager.PixelsPerMeter / 2f - 2f / EyeManager.PixelsPerMeter,
                yOffset / EyeManager.PixelsPerMeter);
            var anchorScreen = args.ViewportControl.WorldToScreen(anchorWorld);
            var textPos = anchorScreen + new Vector2(6f, -textSize.Y - 2f);

            args.ScreenHandle.DrawString(_font, textPos + Vector2.One, text, Color.Black);
            args.ScreenHandle.DrawString(_font, textPos, text, Color.FromHex("#f4f3d0"));
        }
    }
}
