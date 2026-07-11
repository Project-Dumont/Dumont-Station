// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Salvage;

[Prototype]
public sealed partial class SalvageMissionObjectiveHandlerPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField]
    public BaseSalvageMissionObjectiveHandler? Handler;
}