// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using Content.Goobstation.Shared.SpaceWhale;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Goobstation.Client.SpaceWhale;

public sealed partial class TailedEntitySystem : SharedTailedEntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    [SubscribeLocalEvent]
    private void OnAfterAutoHandleState(Entity<TailedEntityComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_spriteQuery.TryGetComponent(ent.Owner, out var sprite))
            sprite.RenderOrder = (uint) ent.Comp.TailSegments.Count + 5;
    }

    [SubscribeLocalEvent]
    private void OnSegmentAfterAutoHandleState(Entity<TailedEntitySegmentComponent> ent,
        ref AfterAutoHandleStateEvent args)
    {
        if (!_spriteQuery.TryGetComponent(ent.Owner, out var sprite))
            return;

        sprite.RenderOrder = (uint) (ent.Comp.SegmentCount - ent.Comp.Order + 5);

        if (ent.Comp.SegmentSpriteState is not { } segmentState || ent.Comp.TailSpriteState is not { } tailState)
            return;

        _sprite.LayerSetRsiState((ent, sprite),
            TailedEntitySegmentLayer.Base,
            ent.Comp.Order == ent.Comp.SegmentCount - 1 ? tailState : segmentState);
    }

    [SubscribeLocalEvent]
    private void OnMove(Entity<TailedEntityComponent> ent, ref MoveEvent args)
    {
        if (args.OldPosition == args.NewPosition && args.OldRotation == args.NewRotation ||
            TerminatingOrDeleted(args.Entity) || ent.Comp.TailSegments.Count == 0)
            return;

        UpdateTailPositions((ent, ent.Comp, args.Entity.Comp1));
    }
}
