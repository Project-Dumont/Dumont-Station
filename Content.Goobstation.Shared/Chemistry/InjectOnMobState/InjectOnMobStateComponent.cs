// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 _kote <143940725+FaDeOkno@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;

namespace Content.Goobstation.Shared.Chemistry.InjectOnMobState;

[RegisterComponent]
public sealed partial class InjectOnMobStateComponent : Component
{
    [DataField]
    public MobState State = MobState.Critical;

    [DataField]
    public Dictionary<string, FixedPoint2> Reagents = new();

    [DataField]
    public float Cooldown = 180;

    public TimeSpan NextUse = TimeSpan.Zero;
}
