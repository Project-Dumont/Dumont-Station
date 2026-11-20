// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.EntityEffects;

public sealed partial class SetBoltsDown : EntityEffectBase<SetBoltsDown>
{
    [DataField]
    public bool Value;
}

public sealed partial class SetBoltsDownEffectSystem : EntityEffectSystem<DoorBoltComponent, SetBoltsDown>
{
    [Dependency] private SharedDoorSystem _door = default!;

    protected override void Effect(Entity<DoorBoltComponent> ent, ref EntityEffectEvent<SetBoltsDown> args)
    {
        _door.SetBoltsDown(ent, args.Effect.Value, args.User, true);
    }
}
