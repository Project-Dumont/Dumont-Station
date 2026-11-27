// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Station;

/// <summary>
/// Station effect that queries all entities with a given component on the station, and applies some entity effects to them.
/// </summary>
public sealed partial class StationQueryEffects : EventEntityEffect<StationQueryEffects>
{
    /// <summary>
    /// Name of the component to query.
    /// </summary>
    [DataField(required: true)]
    public string CompName = string.Empty;

    /// <summary>
    /// The effects to apply to each entity.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;

    /// <summary>
    /// Include paused entities in the query.
    /// Needed if you are running it on a pre-init map.
    /// </summary>
    [DataField]
    public bool IncludePaused;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
