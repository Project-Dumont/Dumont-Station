// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.PathSpecific.Void;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.SpriteOverlay;

public sealed class VoidCurseOverlaySystem : SpriteOverlaySystem<VoidCurseComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidCurseComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        VoidCurseComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);

        var state = comp.Stacks >= comp.MaxStacks ? comp.OverlayStateMax : comp.OverlayStateNormal;

        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
