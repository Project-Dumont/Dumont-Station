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

public sealed partial class ToggleDoorBolts : EntityEffectBase<ToggleDoorBolts>;

public sealed partial class ToggleDoorBoltsEffectSystem : EntityEffectSystem<DoorBoltComponent, ToggleDoorBolts>
{
    [Dependency] private SharedDoorSystem _door = default!;

    protected override void Effect(Entity<DoorBoltComponent> ent, ref EntityEffectEvent<ToggleDoorBolts> args)
    {
        _door.SetBoltsDown(ent, !ent.Comp.BoltsDown, args.User, predicted: args.Predicted);
    }
}
