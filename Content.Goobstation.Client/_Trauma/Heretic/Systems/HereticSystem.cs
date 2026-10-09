// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Systems;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed class HereticSystem : SharedHereticSystem
{
    public override void Initialize()
    {
        base.Initialize();

        UpdatesOutsidePrediction = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        var now = Timing.CurTime;

        var query = EntityQueryEnumerator<HereticSacrificeTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.RemovalTimer)
                continue;

            RemCompDeferred(uid, comp);
        }
    }
}
