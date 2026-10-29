// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.EntityEffects;

public sealed partial class ClearAccesses : EntityEffectBase<ClearAccesses>;

public sealed partial class ClearAccessesEffectSystem : EntityEffectSystem<AccessReaderComponent, ClearAccesses>
{
    [Dependency] private AccessReaderSystem _reader = default!;

    protected override void Effect(Entity<AccessReaderComponent> entity, ref EntityEffectEvent<ClearAccesses> args)
    {
        _reader.ClearAccesses(entity);
    }
}
