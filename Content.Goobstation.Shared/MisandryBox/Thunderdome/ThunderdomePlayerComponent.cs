// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[RegisterComponent]
public sealed partial class ThunderdomePlayerComponent : Component
{
    [DataField]
    public EntityUid? RuleEntity;

    [DataField]
    public int Kills;

    [DataField]
    public int Deaths;

    [DataField]
    public int CurrentStreak;

    [DataField]
    public int WeaponSelection;

    public EntityUid? LastAttacker;
}
