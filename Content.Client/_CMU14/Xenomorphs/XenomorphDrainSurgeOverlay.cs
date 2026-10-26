using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Shared.Utility.SpriteSpecifier;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphDrainSurgeOverlay : Overlay
{
    private static readonly Rsi DripIcon = new(new ResPath("/Textures/_RMC14/Effects/effects.rsi"), "drip");
    private static readonly Rsi ToxicSlashIcon = new(new ResPath("/Textures/_RMC14/Effects/effects.rsi"), "x");
    private const int DrainSurgeParticles = 14;
    private const int ToxicSlashParticles = 18;
    private const int IntoxicatedMinParticles = 8;
    private const int IntoxicatedMaxParticles = 21;
    private const float DrainSurgeRadius = 9f / EyeManager.PixelsPerMeter;
    private const float ToxicSlashRadius = 8f / EyeManager.PixelsPerMeter;
    private const float IntoxicatedRadius = 10f / EyeManager.PixelsPerMeter;

    private readonly IEntityManager _entity;
    private readonly ContainerSystem _container;
    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly EntityQuery<XenomorphInvisibilityComponent> _invisQuery;
    private readonly EntityQuery<TransformComponent> _xformQuery;

    [Dependency] private IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public XenomorphDrainSurgeOverlay(IEntityManager entity)
    {
        IoCManager.InjectDependencies(this);
        _entity = entity;
        _container = entity.System<ContainerSystem>();
        _sprite = entity.System<SpriteSystem>();
        _transform = entity.System<TransformSystem>();
        _invisQuery = entity.GetEntityQuery<XenomorphInvisibilityComponent>();
        _xformQuery = entity.GetEntityQuery<TransformComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var now = (float) _timing.CurTime.TotalSeconds;

        var drainTexture = _sprite.GetFrame(DripIcon, _timing.CurTime);
        var drainQuery = _entity.AllEntityQueryEnumerator<XenomorphDrainSurgeComponent, SpriteComponent, TransformComponent>();
        while (drainQuery.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            if (ShouldSkip(uid, xform, args.MapId))
                continue;

            DrawParticles(
                args,
                uid,
                sprite,
                xform,
                drainTexture,
                DrainSurgeParticles,
                DrainSurgeRadius,
                GetDrainSurgeOffset(uid, sprite, xform),
                new Vector2(0, -0.8f),
                0.6f,
                Color.Chartreuse,
                now);
        }

        var slashTexture = _sprite.GetFrame(ToxicSlashIcon, _timing.CurTime);
        var slashQuery = _entity.AllEntityQueryEnumerator<XenomorphToxicComponent, SpriteComponent, TransformComponent>();
        while (slashQuery.MoveNext(out var uid, out var slash, out var sprite, out var xform))
        {
            if (slash.HitsRemaining <= 0 || slash.SlashUntil <= _timing.CurTime || ShouldSkip(uid, xform, args.MapId))
                continue;

            DrawParticles(
                args,
                uid,
                sprite,
                xform,
                slashTexture,
                ToxicSlashParticles,
                ToxicSlashRadius,
                GetToxicSlashOffset(uid, sprite, xform),
                new Vector2(0, 0.4f),
                0.7f,
                Color.FromHex("#7DCC00"),
                now);
        }

        var intoxicatedQuery = _entity.AllEntityQueryEnumerator<XenomorphIntoxicatedComponent, SpriteComponent, TransformComponent>();
        while (intoxicatedQuery.MoveNext(out var uid, out var intoxicated, out var sprite, out var xform))
        {
            if (ShouldSkip(uid, xform, args.MapId))
                continue;

            var count = Math.Clamp(
                IntoxicatedMinParticles + intoxicated.Stacks / 2,
                IntoxicatedMinParticles,
                IntoxicatedMaxParticles);

            DrawParticles(
                args,
                uid,
                sprite,
                xform,
                slashTexture,
                count,
                IntoxicatedRadius,
                Vector2.Zero,
                new Vector2(0, 0.4f),
                0.65f,
                Color.FromHex("#7DCC00"),
                now);
        }
    }

    private bool ShouldSkip(EntityUid uid, TransformComponent xform, MapId mapId)
    {
        return xform.MapID != mapId ||
               _container.IsEntityOrParentInContainer(uid, xform: xform) ||
               (_invisQuery.TryComp(uid, out var invisibility) && invisibility.Active);
    }

    private Vector2 GetDrainSurgeOffset(EntityUid uid, SpriteComponent sprite, TransformComponent xform)
    {
        var bounds = _sprite.GetLocalBounds((uid, sprite));
        var dir = _transform.GetWorldRotation(xform, _xformQuery).GetCardinalDir();

        var facingOffset = dir switch
        {
            Direction.East => new Vector2(0.12f, 0.02f),
            Direction.West => new Vector2(-0.12f, 0.02f),
            Direction.North => new Vector2(0f, 0.08f),
            Direction.South => new Vector2(0f, -0.04f),
            _ => Vector2.Zero,
        };

        return bounds.Center + new Vector2(0f, bounds.Height * 0.12f) + facingOffset;
    }

    private Vector2 GetToxicSlashOffset(EntityUid uid, SpriteComponent sprite, TransformComponent xform)
    {
        var bounds = _sprite.GetLocalBounds((uid, sprite));
        var dir = _transform.GetWorldRotation(xform, _xformQuery).GetCardinalDir();

        var facingOffset = dir switch
        {
            Direction.East => new Vector2(0.08f, -0.02f),
            Direction.West => new Vector2(-0.08f, -0.02f),
            Direction.North => new Vector2(0f, 0.04f),
            Direction.South => new Vector2(0f, -0.06f),
            _ => Vector2.Zero,
        };

        return bounds.Center + new Vector2(0f, bounds.Height * 0.02f) + facingOffset;
    }

    private void DrawParticles(
        in OverlayDrawArgs args,
        EntityUid uid,
        SpriteComponent sprite,
        TransformComponent xform,
        Robust.Client.Graphics.Texture texture,
        int count,
        float radius,
        Vector2 offset,
        Vector2 gravity,
        float scale,
        Color color,
        float now)
    {
        var bounds = _sprite.GetLocalBounds((uid, sprite));
        var worldPos = _transform.GetWorldPosition(xform, _xformQuery);

        if (!bounds.Translated(worldPos).Intersects(args.WorldAABB))
            return;

        var handle = args.WorldHandle;
        var textureSize = new Vector2(texture.Width, texture.Height) / EyeManager.PixelsPerMeter * scale;

        for (var i = 0; i < count; i++)
        {
            var seed = uid.Id * 397 ^ i * 7919;
            var phase = (now * 0.5f + Hash01(seed)) % 1f;
            var angle = Hash01(seed + 17) * MathF.Tau;
            var particleRadius = MathF.Sqrt(Hash01(seed + 31)) * radius;
            var drift = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * particleRadius;
            var movement = gravity * phase;
            var alpha = MathF.Min(phase, 1f - phase) * 2f;
            var center = worldPos + offset + drift + movement;

            handle.DrawTextureRect(
                texture,
                Box2.CenteredAround(center, textureSize),
                color.WithAlpha(alpha * 0.85f));
        }
    }

    private static float Hash01(int seed)
    {
        unchecked
        {
            var value = (uint) seed;
            value ^= value >> 16;
            value *= 0x7feb352d;
            value ^= value >> 15;
            value *= 0x846ca68b;
            value ^= value >> 16;
            return (value & 0x00ffffff) / 16777215f;
        }
    }
}
