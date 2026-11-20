// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Graphics;
using Content.Trauma.Shared.Scanning;
using Robust.Shared.Timing;

using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;

namespace Content.Trauma.Client.Scanning;

public sealed partial class ScanalyzerVisualsSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private TransformSystem _transform = default!;

    [Dependency] private EntityQuery<ActiveScanComponent> _scannerQuery = default!;

    public static readonly ProtoId<ShaderPrototype> ScanShader = "TraumaScanalyzerScan";
    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlayMan.AddOverlay(new ScanalyzerOverlay());
        _shader = ProtoMan.Index(ScanShader).InstanceUnique();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayMan.RemoveOverlay<ScanalyzerOverlay>();
    }

    [SubscribeLocalEvent]
    private void OnBeforeRender(Entity<ScanalyzerTargetComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        if (args.Id != ScanShader)
            return;

        if (!Exists(ent.Comp.Scanner) || !_scannerQuery.TryComp(ent.Comp.Scanner, out var scanner))
            return;

        var ratio = GetScanRatio(scanner, _timing.CurTime);
        args.Shader.SetParameter("ratio", ratio);
        var zoom = 1f;
        var eyeRot = Angle.Zero;

        if (args.Viewport.Eye is { } eye)
        {
            eyeRot = eye.Rotation;
            zoom = eye.Zoom.X;
        }

        var rot = args.Sprite.Rotation - eyeRot;
        if (!args.Sprite.NoRotation)
            rot -= _transform.GetWorldRotation(ent);

        args.Shader.SetParameter("angle", (float) rot.Theta);
        args.Shader.SetParameter("zoom", zoom);
    }

    [SubscribeLocalEvent]
    private void OnScannedShutdown(Entity<ScanalyzerTargetComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _sprite.RemovePostShader(ent.Owner, ScanShader);
    }

    [SubscribeLocalEvent]
    private void OnScannedStartup(Entity<ScanalyzerTargetComponent> ent, ref ComponentStartup args)
    {
        _sprite.SetPostShader(ent.Owner,
            new(ScanShader, _shader)
            {
                RaiseShaderEvent = true,
                Before = ContentPostShaderIds.BeforeOutlines,
            });
    }

    [SubscribeLocalEvent]
    private void OnScannerShutdown(Entity<ActiveScanComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent.Comp.ScannedObject))
            return;

        if (TryComp<ScanalyzerTargetComponent>(ent.Comp.ScannedObject, out var target) && target.Scanner == ent.Owner)
            RemCompDeferred<ScanalyzerTargetComponent>(ent.Comp.ScannedObject);
    }

    [SubscribeLocalEvent]
    private void OnState(Entity<ActiveScanComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!Exists(ent.Comp.ScannedObject) || !HasComp<SpriteComponent>(ent.Comp.ScannedObject))
            return;

        EnsureComp<ScanalyzerTargetComponent>(ent.Comp.ScannedObject).Scanner = ent;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        var query = EntityQueryEnumerator<ActiveScanComponent>();
        while (query.MoveNext(out var uid, out var scanner))
        {
            if (!Exists(scanner.ScannedObject) || !HasComp<SpriteComponent>(scanner.ScannedObject))
                continue;

            EnsureComp<ScanalyzerTargetComponent>(scanner.ScannedObject).Scanner = uid;
        }
    }

    internal static float GetScanRatio(ActiveScanComponent scanner, TimeSpan now)
    {
        var duration = scanner.ScanEndTime - scanner.ScanStartTime;
        return duration <= TimeSpan.Zero ? 1f : (float) Math.Clamp((now - scanner.ScanStartTime) / duration, 0f, 1f);
    }
}
