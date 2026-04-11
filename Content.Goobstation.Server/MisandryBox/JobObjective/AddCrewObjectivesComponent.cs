// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.MisandryBox.JobObjective;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.MisandryBox.JobObjective;

[RegisterComponent]
public sealed partial class AddCrewObjectivesComponent : Component
{
    /// <summary>
    /// List of objective prototypes to randomly assign
    /// </summary>
    [DataField("objectives", required: true)]
    public List<string> Objectives = new();

    /// <summary>
    /// Number of objectives to randomly select from the list. Defaults to 1.
    /// If greater than available objectives, will select all objectives.
    /// </summary>
    [DataField("count")]
    public int Count = 1;
}
