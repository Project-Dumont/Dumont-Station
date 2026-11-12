// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Station;

/// <summary>
/// Station effect that finds an unblocked area with a given component on the station, then applies some entity effects to it.
/// </summary>
public sealed partial class StationAreaEffects : EventEntityEffect<StationAreaEffects>
{
    /// <summary>
    /// Name of the area's component to query.
    /// </summary>
    [DataField(required: true)]
    public string AreaComp = string.Empty;

    /// <summary>
    /// The effects to apply to the chosen area.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;

    [DataField]
    public int Min = 1;

    [DataField]
    public int Max = 1;

    /// <summary>
    /// Check that valid locations don't have anything blocking mob movement on their tiles.
    /// This includes windoors...
    /// </summary>
    [DataField]
    public bool CheckBlocked = true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
