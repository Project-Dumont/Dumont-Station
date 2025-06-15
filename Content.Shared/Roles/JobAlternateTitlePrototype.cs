// SPDX-FileCopyrightText: 2024 joshepvodka <guilherme.ornel@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

[Prototype("jobAlternateTitle")]
public sealed partial class JobAlternateTitlePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = default!;

    public string LocalizedName => Loc.GetString(Name);
}
