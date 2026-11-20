// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Systems;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.SpriteOverlay;

public sealed class HereticCombatMarkOverlaySystem : SpriteOverlaySystem<HereticCombatMarkComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
        SubscribeLocalEvent<HereticCombatMarkComponent, UpdateCombatMarkAppearanceEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }

    protected override int? GetLayerIndex(Entity<SpriteComponent> ent, HereticCombatMarkComponent comp)
    {
        return comp.Path == HereticPath.Cosmos ? 0 : null; // Cosmos mark should be behind the sprite
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        HereticCombatMarkComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);

        var state = comp.Path.ToString().ToLower();

        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
