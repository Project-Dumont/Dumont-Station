// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Scanning;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;

namespace Content.Trauma.Client.Scanning;

public sealed partial class ScanalyzerOverlay : Overlay
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private TransformSystem _xform;
    private SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private readonly List<Vector2> _vertices = new();

    private readonly ShaderInstance _unshadedShader;

    public static readonly ProtoId<ShaderPrototype> Unshaded = "unshaded";

    public ScanalyzerOverlay()
    {
        IoCManager.InjectDependencies(this);

        _xform = _entMan.System<TransformSystem>();
        _sprite = _entMan.System<SpriteSystem>();

        _unshadedShader = _proto.Index(Unshaded).Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye is not { } eye)
            return;

        var handle = args.WorldHandle;

        var beingScannedQuery = _entMan.GetEntityQuery<ScanalyzerTargetComponent>();
        var xformQuery = _entMan.GetEntityQuery<TransformComponent>();
        var spriteQuery = _entMan.GetEntityQuery<SpriteComponent>();

        _vertices.Clear();
        var query = _entMan.EntityQueryEnumerator<ActiveScanComponent, TransformComponent>();
        while (query.MoveNext(out var scanner, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var scanned = scanner.ScannedObject;
            if (!_entMan.EntityExists(scanned) || !beingScannedQuery.TryComp(scanned, out _)
                || !spriteQuery.TryComp(scanned, out var sprite))
                continue;

            var ourPos = _xform.GetWorldPosition(xform, xformQuery);
            var (pos, rot) = _xform.GetWorldPositionRotation(scanned);
            var spriteBB = _sprite.CalculateBounds((scanned, sprite), pos, rot, Angle.Zero);

            var ratio = ScanalyzerVisualsSystem.GetScanRatio(scanner, _timing.CurTime);
            _vertices.Add(ourPos);
            _vertices.Add(Vector2.Lerp(spriteBB.TopLeft, spriteBB.TopRight, ratio));
            _vertices.Add(Vector2.Lerp(spriteBB.BottomLeft, spriteBB.BottomRight, ratio));

        }

        handle.UseShader(_unshadedShader);
        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, _vertices, Color.Red.WithAlpha(0.1f));
        handle.UseShader(null);
    }
}
