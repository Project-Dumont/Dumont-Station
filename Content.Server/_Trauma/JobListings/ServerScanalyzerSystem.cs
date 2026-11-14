// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;
using Content.Trauma.Shared.JobListings;

namespace Content.Trauma.Server.JobListings;

public sealed partial class ServerScanalyzerSystem : ScanalyzerSystem
{
    [Dependency] private Content.Server.GameTicking.GameTicker _ticker = default!;

    [SubscribeLocalEvent]
    private void OnBroadcast(Entity<BroadcastGameRuleOnScanComponent> ent, ref ScanalyzerScanFinishedEvent args)
    {
        var started = _ticker.StartGameRule(ent.Comp.GameRule, out var rule);
        if (!started || HasComp<EndedGameRuleComponent>(rule) ||
            !TryComp<RuleGridsComponent>(rule, out var grids) || grids.MapGrids.Count == 0)
        {
            args.Cancelled = true;
            Log.Error($"Battlecruiser broadcast failed: rule={ToPrettyString(rule)}, started={started}.");
            _ticker.EndGameRule(rule);
            return;
        }

        Log.Info($"Battlecruiser broadcast loaded {grids.MapGrids.Count} grid(s) for {ToPrettyString(rule)}.");

    }

    [SubscribeLocalEvent(after: [typeof(StealConditionSystem)])]
    private void OnGetProgress(Entity<StealConditionRequireScanComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0.0f;
        if (!TryComp<StealConditionComponent>(ent.Owner, out var stealComp))
            return;

        if (stealComp.StealGroup is not { } groupId)
        {
            Log.Error($"Steal Objective {ent}'s StealGroup field was null.");
            return;
        }

        if (IsScanned((args.MindId, args.Mind), groupId))
            args.Progress = 1.0f;
    }

    [SubscribeLocalEvent]
    private void OnToolSpawned(Entity<ScanalyzerComponent> ent, ref SideJobToolSpawned args)
    {
        if (!TryComp<StealConditionComponent>(args.Objective, out var objComp))
            return;

        ent.Comp.StealTarget ??= objComp.StealGroup;
        Dirty(ent);
    }
}
