using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphBarrageChargeOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        if (!_overlay.HasOverlay<XenomorphBarrageChargeOverlay>())
            _overlay.AddOverlay(new XenomorphBarrageChargeOverlay());
    }

    public override void Shutdown()
    {
        _overlay.RemoveOverlay<XenomorphBarrageChargeOverlay>();
    }
}

public sealed partial class XenomorphBarrageChargeOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private static readonly ResPath BarSprite = new("/Textures/Interface/Misc/progress_bar.rsi");
    private static readonly Color BarFill = Color.Chartreuse;

    private const float StartX = 2f;
    private const float EndX = 22f;
    private const float FillTop = 3f;
    private const float FillBottom = 4f;
    private const float SpriteCullPad = 2f;
    private const float SpriteHeadroom = 0.05f;

    [Dependency] private IEntityManager _entity = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly EntityQuery<TransformComponent> _xformQuery;

    private readonly Texture _barTexture;
    private readonly ShaderInstance _unshaded;
    private readonly float _barHalfWidth;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public XenomorphBarrageChargeOverlay()
    {
        IoCManager.InjectDependencies(this);
        _sprite = _entity.System<SpriteSystem>();
        _transform = _entity.System<TransformSystem>();
        _xformQuery = _entity.GetEntityQuery<TransformComponent>();
        _barTexture = _sprite.Frame0(new SpriteSpecifier.Rsi(BarSprite, "icon"));
        _unshaded = _proto.Index(UnshadedShader).Instance();
        _barHalfWidth = _barTexture.Width / 2f / EyeManager.PixelsPerMeter;
        ZIndex = 1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var eyeRot = args.Viewport.Eye?.Rotation ?? default;
        var rotation = Matrix3Helpers.CreateRotation(-eyeRot);
        var localEnt = _player.LocalSession?.AttachedEntity;
        var now = _timing.CurTime;
        var cullBounds = args.WorldAABB.Enlarged(SpriteCullPad);

        var query = _entity.AllEntityQueryEnumerator<XenomorphDespoilerComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var charge, out var sprite, out var xform))
        {
            if (!charge.Charging || xform.MapID != args.MapId)
                continue;

            var worldPos = _transform.GetWorldPosition(xform, _xformQuery);
            if (!cullBounds.Contains(worldPos))
                continue;

            handle.UseShader(uid == localEnt ? _unshaded : null);

            var world = Matrix3Helpers.CreateTranslation(worldPos);
            handle.SetTransform(Matrix3x2.Multiply(rotation, world));

            var alpha = sprite.Color.A;
            var yOffset = _sprite.GetLocalBounds((uid, sprite)).Height / 2f + SpriteHeadroom;
            var origin = new Vector2(-_barHalfWidth, yOffset);

            handle.DrawTexture(_barTexture, origin, Color.White.WithAlpha(alpha));

            var duration = 5.0;
            var ratio = duration > 0
                ? (float)Math.Clamp((now - charge.ChargeStartedAt).TotalSeconds / duration, 0, 1)
                : 1f;

            var xProgress = (EndX - StartX) * ratio + StartX;
            var fill = new Box2(
                new Vector2(StartX, FillTop) / EyeManager.PixelsPerMeter,
                new Vector2(xProgress, FillBottom) / EyeManager.PixelsPerMeter)
                .Translated(origin);

            handle.DrawRect(fill, BarFill.WithAlpha(alpha));
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
