// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Movement.Components;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.AudioMuffle;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
using Robust.Client.Player;
using Robust.Shared.Enums;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
// Dumont end

// Dumont start
using Content.Trauma.Common.Sprite;
// Dumont end

namespace Content.Trauma.Client.Heretic;

public sealed partial class DigitalCamouflageOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace | OverlaySpace.WorldSpaceBelowEntities;

    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPlayerManager _player = default!;
    private readonly CommonSpriteVisibilitySystem _spriteVis;

    private readonly HashSet<Entity<SpriteComponent>> _hiddenEntities = new();

    public DigitalCamouflageOverlay()
    {
        IoCManager.InjectDependencies(this);

        _spriteVis = _entMan.System<CommonSpriteVisibilitySystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Space == OverlaySpace.ScreenSpace)
            return true;

        if (_player.LocalEntity is not { } player)
            return false;

        if (args.Viewport.Eye != _eye.CurrentEye)
            return true;

        return _entMan.TryGetComponent(player, out RelayInputMoverComponent? relay) &&
               _entMan.HasComponent<AiEyeComponent>(relay.RelayEntity);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Space == OverlaySpace.ScreenSpace)
        {
            foreach (var ent in _hiddenEntities)
            {
                _spriteVis.UpdateVisibilityModifiers(ent, nameof(DigitalCamouflageComponent), 1f);
            }

            _hiddenEntities.Clear();
            return;
        }

        var query = _entMan.EntityQueryEnumerator<SpriteComponent>();
        while (query.MoveNext(out var uid, out var sprite))
        {
            var ev = new CanSeeOnCameraEvent(uid);
            _entMan.EventBus.RaiseLocalEvent(uid, ref ev);
            if (!ev.Cancelled)
                continue;

            _spriteVis.UpdateVisibilityModifiers(uid, nameof(DigitalCamouflageComponent), 0f);
            _hiddenEntities.Add((uid, sprite));
        }
    }
}
