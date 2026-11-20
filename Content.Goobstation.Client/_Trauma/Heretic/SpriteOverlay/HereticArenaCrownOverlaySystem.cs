// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.SpriteOverlay;

public sealed class HereticArenaCrownOverlaySystem : SpriteOverlaySystem<HereticArenaParticipantComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticArenaParticipantComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        HereticArenaParticipantComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);

        var state = comp.IsVictor ? comp.VictorState : comp.FighterState;

        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
