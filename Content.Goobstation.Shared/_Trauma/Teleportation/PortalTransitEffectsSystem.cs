// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Trauma.Common.Teleportation;
// Dumont end

namespace Content.Trauma.Shared.Teleportation;

public sealed partial class PortalTransitEffectsSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [SubscribeLocalEvent]
    private void OnPortalTeleported(Entity<PortalTransitEffectsComponent> ent, ref PortalTeleportedEvent args)
    {
        if (ent.Comp.Effects is { } effects)
            _effects.ApplyEffects(ent, effects);
        if (ent.Comp.SourceEffects is { } srcEffects)
            _effects.ApplyEffects(args.Source, srcEffects);
        if (ent.Comp.DestEffects is { } destEffects && args.Dest is { } dest)
            _effects.ApplyEffects(dest, destEffects);
    }
}
