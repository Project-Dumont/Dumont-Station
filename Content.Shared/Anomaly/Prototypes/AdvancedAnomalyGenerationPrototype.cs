// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Anomaly.Prototypes;

[Prototype("advancedAnomalyGeneration")]
public sealed class AdvancedAnomalyGenerationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = string.Empty;

    [DataField(required: true)]
    public EntProtoId AnomalyPrototype { get; private set; } = default!;

    [DataField]
    public int ResearchCost { get; private set; }

    [DataField]
    public int? PlasmaCost { get; private set; }
}
