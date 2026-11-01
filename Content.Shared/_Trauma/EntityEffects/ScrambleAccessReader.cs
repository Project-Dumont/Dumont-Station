// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Clears the target entity's access reader lists, then adds some random levels back.
/// </summary>
public sealed partial class ScrambleAccessReader : EventEntityEffect<ScrambleAccessReader>
{
    [DataField]
    public int Min = 1;

    [DataField]
    public int Max = 3;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
