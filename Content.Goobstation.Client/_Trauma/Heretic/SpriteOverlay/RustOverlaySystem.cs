// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Tag;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.SpriteOverlay;

public sealed partial class RustOverlaySystem : SpriteOverlaySystem<RustOverlayComponent>
{
    [Dependency] private TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RustOverlayComponent, IconSmoothCornersInitializedEvent>(OnIconSmoothInit);
    }

    private void OnIconSmoothInit(Entity<RustOverlayComponent> ent, ref IconSmoothCornersInitializedEvent args)
    {
        RemoveOverlay(ent.Owner, ent.Comp);
        AddOverlay(ent.Owner, ent.Comp);
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        RustOverlayComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);

        var diagonal = _tag.HasTag(ent, comp.DiagonalTag);
        var state = diagonal ? comp.DiagonalState : comp.OverlayState;

        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
