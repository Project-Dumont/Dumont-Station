// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Conditions;

/// <summary>
/// Condition that requires the target player spawn point has a non-null <c>Job</c> set.
/// </summary>
public sealed partial class SpawnPointHasJob : EventEntityEffectCondition<SpawnPointHasJob>
{
    public override string GuidebookExplanation(IPrototypeManager prototype)
        => string.Empty;
}
