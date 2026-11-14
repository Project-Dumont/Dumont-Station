// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.EntityEffects;

public sealed partial class SetThrowCooldown : EntityEffectBase<SetThrowCooldown>
{
    [DataField(required: true)]
    public TimeSpan Cooldown;
}

public sealed partial class SetThrowCooldownEffectSystem : EntityEffectSystem<HandsComponent, SetThrowCooldown>
{
    [Dependency] private IGameTiming _timing = default!;

    protected override void Effect(Entity<HandsComponent> ent, ref EntityEffectEvent<SetThrowCooldown> args)
    {
        // Dumont start
        EntityManager.System<Content.Shared.Hands.EntitySystems.SharedHandsSystem>()
            .SetNextThrowTime(ent, _timing.CurTime + args.Effect.Cooldown);
        // Dumont end
    }
}
