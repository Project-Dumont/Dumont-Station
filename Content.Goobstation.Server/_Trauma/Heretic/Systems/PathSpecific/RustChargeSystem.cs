// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Server.Destructible;
using Content.Shared.Destructible;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Rust;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems.PathSpecific;

public sealed partial class RustChargeSystem : SharedRustChargeSystem
{
    [Dependency] private DestructibleSystem _destructible = default!;

    protected override void DestroyStructure(EntityUid uid, EntityUid user)
    {
        base.DestroyStructure(uid, user);

        if (TryComp(uid, out RustRequiresPathStageComponent? rusty) && rusty.PathStage > 10)
            return;

        if (!TryComp(uid, out DestructibleComponent? destructible) || destructible.Thresholds.Count == 0)
        {
            Del(uid);
            return;
        }

        var threshold = destructible.Thresholds[^1];
        RaiseLocalEvent(uid, new DamageThresholdReached(destructible, threshold), true);
        threshold.Execute(uid, _destructible, EntityManager, user);
    }
}
