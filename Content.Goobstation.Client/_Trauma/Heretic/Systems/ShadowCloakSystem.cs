// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared._Goobstation.Wizard.Traps;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using System.Linq;
using Content.Trauma.Client.Heretic.SpriteOverlay;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Void;
using Content.Trauma.Shared.Heretic.Components.Side;
using Content.Trauma.Shared.Heretic.Systems.Side;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

// Dumont start
using Content.Trauma.Common.Sprite;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed partial class ShadowCloakSystem : SharedShadowCloakSystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private CommonSpriteVisibilitySystem _spriteVis = default!;

    private TimeSpan _nextUpdate;

    private static readonly TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<EntropicPlumeAffectedComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<FireBlastedComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<HereticCombatMarkComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<HereticEyeOverlayComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<SacramentsOfPowerComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<StarMarkComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<VoidCurseComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<HereticArenaParticipantComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<AimedRifleMarkerComponent>>(UpdateOverlay);
        SubscribeLocalEvent<ShadowCloakedComponent, SpriteOverlayUpdatedEvent<UnfathomableCurioShieldComponent>>(UpdateOverlay);

        UpdatesOutsidePrediction = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = Timing.CurTime;

        if (now < _nextUpdate)
            return;

        _nextUpdate = now + UpdateDelay;

        // Sync post shaders user -> cloak
        var query = EntityQueryEnumerator<ShadowCloakEntityComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var cloak, out var sprite))
        {
            if (cloak.User is not { } user || !Exists(user))
                return;

            var toRemove = _sprite.GetPostShaders((uid, sprite)).Select(x => x.Id).ToList();
            // Dumont start
            var hasIceLayer = _sprite.LayerMapTryGet((uid, sprite), IceCubeKey.Key, out var iceLayer, false);
            if (TryComp<IceCubeComponent>(user, out var ice))
            {
                if (!hasIceLayer)
                {
                    iceLayer = _sprite.AddLayer((uid, sprite), ice.Sprite);
                    _sprite.LayerMapSet((uid, sprite), IceCubeKey.Key, iceLayer);
                }
            }
            else if (hasIceLayer)
                _sprite.RemoveLayer((uid, sprite), iceLayer);
            // Dumont end
            foreach (var shader in _sprite.GetPostShaders(user))
            {
                // Dumont start
                if (shader.Id == "__legacy")
                    continue;
                // Dumont end
                // Don't raise shader event cause it is already being raised on user and shader instance is the same
                var args = new SpriteComponent.PostShaderArgs(shader.Id, shader.Shader)
                {
                    Before = shader.Before,
                    After = shader.After,
                };
                _sprite.SetPostShader((uid, sprite), args);
                toRemove.Remove(shader.Id);
            }

            foreach (var shader in toRemove)
            {
                // Don't remove outlines
                if ((shader == "__legacy" || Content.Client.Graphics.ContentPostShaderIds.BeforeOutlines.Contains(shader)))
                    continue;

                _sprite.RemovePostShader((uid, sprite), shader);
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnEntityStartup(Entity<ShadowCloakEntityComponent> ent, ref ComponentStartup args)
    {
        if (!Exists(ent.Comp.User))
            return;

        // Update visual appearance
        if (TryComp(ent.Comp.User.Value, out SpriteComponent? sprite))
            _appearance.OnChangeData(ent.Comp.User.Value, sprite);
    }

    private void UpdateOverlay<T>(Entity<ShadowCloakedComponent> ent, ref SpriteOverlayUpdatedEvent<T> args)
        where T : BaseSpriteOverlayComponent
    {
        if (GetShadowCloakEntity(ent) is not { } cloak)
            return;

        if (args.Added)
            args.Sys.AddOverlay(cloak.Owner, args.Comp, ent);
        else
            args.Sys.RemoveOverlay(cloak.Owner, args.Comp);
    }

    protected override void Startup(Entity<ShadowCloakedComponent> ent)
    {
        base.Startup(ent);

        _spriteVis.UpdateVisibilityModifiers(ent, nameof(ShadowCloakedComponent), 0f);
    }

    protected override void Shutdown(Entity<ShadowCloakedComponent> ent)
    {
        base.Shutdown(ent);

        _spriteVis.UpdateVisibilityModifiers(ent, nameof(ShadowCloakedComponent), 1f);
    }
}
