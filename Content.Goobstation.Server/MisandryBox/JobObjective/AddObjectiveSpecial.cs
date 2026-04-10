// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Roles;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.MisandryBox.JobObjective;

public sealed partial class AddObjectiveSpecial : JobSpecial
{
    /// <summary>
    /// List of objective prototypes to randomly assign to this job
    /// </summary>
    [DataField("objectives", required: true)]
    public List<string> Objectives = new();

    /// <summary>
    /// Number of objectives to randomly select from the list. Defaults to 1.
    /// If greater than available objectives, will select all objectives.
    /// </summary>
    [DataField("count")]
    public int Count = 1;

    public override void AfterEquip(EntityUid mob)
    {
        if (Objectives.Count == 0)
            return;

        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<JobObjectiveSystem>();
        var random = IoCManager.Resolve<IRobustRandom>();
        
        var count = Math.Min(Count, Objectives.Count);
        var shuffled = new List<string>(Objectives);
        random.Shuffle(shuffled);
        var selectedObjectives = shuffled.Take(count).ToList();
        
        system.QueueObjectives(mob, selectedObjectives);
    }
}
