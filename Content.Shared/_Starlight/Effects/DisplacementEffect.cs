// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Rinary <rinary.super@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.DisplacementMap;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared._Starlight.Effects;

[Prototype("displacementEffect")]
public sealed partial class DisplacementEffect : IPrototype
{
    [IdDataField] public string ID { get; } = null!;

    [DataField("displacement", required: true)] public DisplacementData Displacement = null!;
}
