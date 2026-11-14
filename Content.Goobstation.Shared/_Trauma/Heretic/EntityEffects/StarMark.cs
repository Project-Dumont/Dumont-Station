// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Shared.Mobs.Components;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Cosmos;
// Dumont end

namespace Content.Trauma.Shared.Heretic.EntityEffects;

public sealed partial class StarMark : EntityEffectBase<StarMark>;

public sealed partial class StarMarkEffectSystem : EntityEffectSystem<MobStateComponent, StarMark>
{
    [Dependency] private SharedStarMarkSystem _starMark = default!;

    protected override void Effect(Entity<MobStateComponent> ent, ref EntityEffectEvent<StarMark> args)
    {
        _starMark.TryApplyStarMark(ent.AsNullable());
    }
}
