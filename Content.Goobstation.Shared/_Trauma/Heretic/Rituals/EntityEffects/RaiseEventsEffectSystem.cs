// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed class RaiseEventsEffectSystem : EntityEffectSystem<MetaDataComponent, RaiseEvents>
{
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<RaiseEvents> args)
    {
        foreach (var ev in args.Effect.Events)
        {
            RaiseLocalEvent(entity, ev, true);
        }
    }
}
public sealed partial class RaiseEvents : EntityEffectBase<RaiseEvents>
{
    [DataField(required: true), NonSerialized]
    public object[] Events = default!;
}
