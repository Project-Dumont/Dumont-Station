// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Server.Atmos.EntitySystems;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Ash;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems.PathSpecific;

public sealed partial class ScorchedMantleSystem : SharedScorchedMantleSystem
{
    [Dependency] private FlammableSystem _flammable = default!;

    protected override void UpdateFirestacks(EntityUid uid)
    {
        base.UpdateFirestacks(uid);

        _flammable.SetFireStacks(uid, 0.1f, ignite: true);
    }
}
