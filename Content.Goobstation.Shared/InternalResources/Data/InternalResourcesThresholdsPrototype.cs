// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.Data;

/// <summary>
/// Prototype for an internal resource's thresholds.
/// </summary>
[Prototype]
public sealed partial class InternalResourcesThresholdsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Float is the percentage from 0 to 1.
    /// Bool is if the threshold was met.
    /// </summary>
    [DataField]
    public Dictionary<string, (float, bool)>? Thresholds;
}
