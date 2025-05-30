// SPDX-FileCopyrightText: 2024 Lgibb18 <65973111+Lgibb18@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Sunrise.Disease;

[RegisterComponent, Access(typeof(DiseaseRoleSystem))]
public sealed partial class DiseaseRuleComponent : Component
{
    public List<(EntityUid, string)> DiseasesMinds = new();
}
