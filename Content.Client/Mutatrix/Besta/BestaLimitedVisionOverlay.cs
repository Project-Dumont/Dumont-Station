// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Besta.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.Mutatrix.Besta;

/// <summary>
/// Dark radial vision for the Mutatrix Besta.
/// Uses the existing CircleMask shader without the greyscale/blurry blindness shader.
/// </summary>
public sealed class BestaLimitedVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> CircleShader = "CircleMask";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _circleMaskShader;
    private BestaLimitedVisionComponent? _vision;

    public BestaLimitedVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        // Draw early so other world-space overlays, like thermal effects, have a chance to draw over it.
        ZIndex = -50;
        _circleMaskShader = _prototypeManager.Index(CircleShader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var playerEntity = _playerManager.LocalSession?.AttachedEntity;
        if (playerEntity == null)
            return false;

        if (!_entityManager.TryGetComponent(playerEntity.Value, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        if (!_entityManager.TryGetComponent(playerEntity.Value, out BestaLimitedVisionComponent? vision))
            return false;

        _vision = vision;
        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var playerEntity = _playerManager.LocalSession?.AttachedEntity;
        if (playerEntity == null || _vision == null)
            return;

        if (!_entityManager.TryGetComponent<EyeComponent>(playerEntity.Value, out var eye))
            return;

        _circleMaskShader.SetParameter("Zoom", eye.Zoom.X);
        _circleMaskShader.SetParameter("CircleRadius", _vision.RadiusPixels);
        _circleMaskShader.SetParameter("CircleMinDist", _vision.InnerRadiusPixels);
        // Fix33: visão limitada com raio maior, escuro fora do círculo.
        _circleMaskShader.SetParameter("CirclePow", 0.35f);
        _circleMaskShader.SetParameter("CircleMax", 8.0f);
        _circleMaskShader.SetParameter("CircleMult", 1.0f);

        var handle = args.WorldHandle;
        handle.UseShader(_circleMaskShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
