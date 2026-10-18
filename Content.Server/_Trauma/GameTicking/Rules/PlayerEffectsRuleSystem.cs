// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Dumont.EntityEffects;
using Content.Shared.GameTicking;
using Content.Shared.Roles; // Dumont
using Content.Trauma.Server.GameTicking.Rules.Components;
using Robust.Shared.Prototypes; // Dumont

namespace Content.Trauma.Server.GameTicking.Rules;

public sealed partial class PlayerEffectsRuleSystem : EntitySystem
{
    [Dependency] private EntityEffectListSystem _effects = default!;
    [Dependency] private IPrototypeManager _proto = default!; // Dumont

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var query = EntityQueryEnumerator<PlayerEffectsRuleComponent>();
        while (query.MoveNext(out var comp))
        {
            if (comp.Jobs is {} jobs && (args.JobId is not {} job || !jobs.Contains(job)))
                continue;

            // Dumont changes start
            if (comp.Departments is {} departments && !InDepartments(args.JobId, departments))
                continue;
            // Dumont end

            _effects.ApplyEffects(args.Mob, comp.Effects);
        }
    }

    // Dumont changes start
    private bool InDepartments(string? job, HashSet<ProtoId<DepartmentPrototype>> departments)
    {
        if (job == null)
            return false;

        foreach (var id in departments)
        {
            if (_proto.Index(id).Roles.Contains(job))
                return true;
        }

        return false;
    }
    // Dumont end
}
