// SPDX-FileCopyrightText: 2024 Lgibb18 <65973111+Lgibb18@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// © SUNRISE, An EULA/CLA with a hosting restriction, full text: https://github.com/space-sunrise/space-station-14/blob/master/CLA.txt
using System.Numerics;

namespace Content.Server.Traits.Assorted;

[RegisterComponent, Access(typeof(NarcolepsySystem), typeof(SleepySystem))]
public sealed partial class SleepyComponent : Component
{
    [DataField("timeBetweenIncidents", required: true)]
    public Vector2 TimeBetweenIncidents = new Vector2(300, 600);

    [DataField("durationOfIncident", required: true)]
    public Vector2 DurationOfIncident = new Vector2(10, 30);

    public float NextIncidentTime;
}
