// SPDX-FileCopyrightText: 2025 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-License-Identifier: AGPL-3.0-only

using System.Linq;
using Content.Server._DV.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Silicons.Laws;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Server._DV.Objectives.Systems;

public sealed partial class AILawsUpdatedRequirementSystem : EntitySystem
{
    [Dependency] private CodeConditionSystem _code = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedJobSystem _job = default!;
    [Dependency] private SiliconLawSystem _laws = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AILawsUpdatedRequirementComponent, RequirementCheckEvent>(OnRequirementCheck);
        SubscribeLocalEvent<SiliconLawBoundComponent, AILawUpdatedEvent>(OnLawInserted);
    }

    private void OnRequirementCheck(Entity<AILawsUpdatedRequirementComponent> ent, ref RequirementCheckEvent args)
    {
        var query = EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var uid, out var mind))
        {
            if (mind.OwnedEntity != null && !_mind.IsCharacterDeadIc(mind) && _job.MindHasJobWithId(uid, "StationAi"))
                return;
        }
        args.Cancelled = true;
    }

    private void OnLawInserted(Entity<SiliconLawBoundComponent> ent, ref AILawUpdatedEvent args)
    {
        if (!_mind.TryGetMind(ent, out var mind, out var comp) || _mind.IsCharacterDeadIc(comp) ||
            !_job.MindHasJobWithId(mind, "StationAi"))
            return;

        var current = _laws.GetLaws(ent, ent.Comp).Laws;
        var query = EntityQueryEnumerator<AILawsUpdatedRequirementComponent>();
        while (query.MoveNext(out var uid, out var objective))
        {
            // Dumont - the local event has no lawset ID, so check the laws actually installed.
            if (current.SequenceEqual(_laws.GetLawset(objective.Lawset).Laws))
                _code.SetCompleted(uid);
        }
    }
}
