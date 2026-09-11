// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;
using Content.Trauma.Shared.Heretic.Events;
// Dumont end

namespace Content.Trauma.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    [SubscribeLocalEvent]
    private void OnDomainExpansion(EventHereticDomainExpansion args)
    {
        DebugTools.Assert(args.MinRadius <= args.TileRadius);

        var uid = args.Performer;

        if (!TryUseAbility(args, false) || !Heretic.TryGetHereticComponent(uid, out var heretic, out var mind))
            return;

        var coords = Transform(uid).Coordinates;

        var query = EntityQueryEnumerator<BladeArenaComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var xform))
        {
            if (!_transform.InRange(coords, xform.Coordinates, args.TileRadius * 2.5f))
                continue;

            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-arena-nearby"), uid, uid);
            return;
        }

        if (_arena.TrySpawnArena(coords, args.Arena, args.TileReplacement, args.MinRadius, args.TileRadius) is not
            { } arena)
        {
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-not-enough-space"), uid, uid);
            return;
        }

        heretic.Minions.Add(arena);
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnFuriousSteel(EventHereticFuriousSteel args)
    {
        if (!TryUseAbility(args))
            return;

        StatusNew.TryUpdateStatusEffectDuration(args.Performer, args.StatusEffect, out _, args.StatusDuration);
    }
}
