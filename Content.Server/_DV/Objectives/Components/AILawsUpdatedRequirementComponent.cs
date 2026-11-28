// SPDX-FileCopyrightText: 2025 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-License-Identifier: AGPL-3.0-only

using Content.Shared.Silicons.Laws;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

[RegisterComponent]
public sealed partial class AILawsUpdatedRequirementComponent : Component
{
    /// <summary>
    ///     The lawset that is needed to complete the objective.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawsetPrototype> Lawset;
}
