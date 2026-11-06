// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Server._Omu.Objectives.Systems;

namespace Content.Server._Omu.Objectives.Components;

/// <summary>
///     Requires that a cybernetic mantle is present on station.
/// </summary>
[RegisterComponent, Access(typeof(CyberneticMantleExistsOnStationRequirementSystem))]
public sealed partial class CyberneticMantleExistsOnStationRequirementComponent : Component
{
}
