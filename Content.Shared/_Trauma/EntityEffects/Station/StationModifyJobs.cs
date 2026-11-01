// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Station;

/// <summary>
/// Station effect that modifies its job slots.
/// </summary>
public sealed partial class StationModifyJobs : EventEntityEffect<StationModifyJobs>
{
    /// <summary>
    /// Adds or removes slots for a job.
    /// Does nothing to overflow jobs.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, int> Add = new();

    /// <summary>
    /// Sets the slots for a job to a fixed value.
    /// Overrides overflow jobs.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, int> Set = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
