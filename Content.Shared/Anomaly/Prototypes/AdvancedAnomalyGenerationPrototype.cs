// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Anomaly.Prototypes;

[Prototype]
public sealed partial class AdvancedAnomalyGenerationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public EntProtoId AnomalyPrototype;

    [DataField]
    public int ResearchCost;

    // How much of the required material it uses, Doesnt have to be plasma
    [DataField]
    public int MaterialCost = 1500;
}
