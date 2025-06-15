// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(CombatMetricSystem))]
public sealed partial class CombatMetricComponent : Component
{

    /// <summary>
    /// Funky: The rough combat potential of a carp
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double HostileScore = 5.0f;

    /// <summary>
    /// Funky: The rough combat potential of an (unrobust) friendly tider (was 10.0 with Goob)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double FriendlyScore = 2.0f;

    /// <summary>
    ///   Cost per point of medical damage for friendly entities
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double MedicalMultiplier = 0.05f;

    /// <summary>
    ///   Cost for friendlies who are in crit
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double CritScore = 2.0f;

    /// <summary>
    ///   Cost for friendlies who are dead
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double DeadScore = 10.0f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public double maxItemThreat = 15.0f;

    /// <summary>
    ///   ItemThreat - evaluate based on item tags how powerful a player is
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, double> ItemThreat =
        new()
        {
            { "Taser", 3.0f },
            { "Sidearm", 3.0f },
            { "Rifle", 5.0f },
            { "HighRiskItem", 4.0f },
            { "CombatKnife", 2.0f },
            { "Knife", 1.5f },
            { "Grenade", 2.0f },
            { "Bomb", 4.0f },
            { "MagazinePistol", 1.0f },
            { "Hacking", 1.0f },
            { "Jetpack", 1.0f },
            { "Armor", 3.0f},
            { "SpecialArmor", 6.0f},
            { "SpecialWeapon", 6.0f},
        };

}
