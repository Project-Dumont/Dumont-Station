// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Server.Cargo.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
///     Handles the Hijack Trade Station objective.
/// </summary>
public sealed partial class HijackTradeStationConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HijackTradeStationConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<HijackTradeStationConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var enumerator = EntityQueryEnumerator<TradeStationComponent>();
        args.Progress = 0f;
        // If there's any hacked trade station, succeed.
        while (enumerator.MoveNext(out var comp))
        {
            if (!comp.Hacked)
                continue;

            args.Progress = 1f;
            return;
        }
    }
}
