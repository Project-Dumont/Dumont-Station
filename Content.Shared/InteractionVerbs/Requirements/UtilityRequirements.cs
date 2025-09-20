// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Requirements;

[Serializable, NetSerializable]
public sealed partial class ChanceRequirement : InteractionRequirement
{
    [DataField(required: true)]
    public float Chance;

    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        return Chance > 0f && (Chance > 1f || deps.Random.Prob(Chance));
    }
}
