// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Trauma.Common.EntityEffects;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals.EntityEffects;

public sealed partial class RaiseNetworkEventsEffectSystem : EntityEffectSystem<MetaDataComponent, RaiseNetworkEvents>
{
    [Dependency] private INetManager _net = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<RaiseNetworkEvents> args)
    {
        if (_net.IsClient)
            return;

        var netEnt = GetNetEntity(entity, entity.Comp);

        foreach (var ev in args.Effect.Events)
        {
            ev.Entity = netEnt;
            RaiseNetworkEvent(ev);
        }
    }
}
public sealed partial class RaiseNetworkEvents : EntityEffectBase<RaiseNetworkEvents>
{
    [DataField(required: true), NonSerialized]
    public EntityEffectNetworkEvent[] Events = default!;
}
