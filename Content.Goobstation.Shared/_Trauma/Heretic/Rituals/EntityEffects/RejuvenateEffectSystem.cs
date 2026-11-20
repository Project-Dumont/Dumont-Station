// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Shared.Rejuvenate;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed class RejuvenateEffectSystem : EntityEffectSystem<MetaDataComponent, Rejuvenate>
{
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<Rejuvenate> args)
    {
        RaiseLocalEvent(entity, new RejuvenateEvent(args.Effect.Uncuff, args.Effect.ResetActions));
    }
}

public sealed partial class Rejuvenate : EntityEffectBase<Rejuvenate>
{
    [DataField]
    public bool ResetActions = true;

    [DataField]
    public bool Uncuff = true;
}
