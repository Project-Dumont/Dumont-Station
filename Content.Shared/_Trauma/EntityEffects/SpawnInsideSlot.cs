// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Spawns entities inside an entity's inventory slot's storage.
/// Drops it next to them if that fails.
/// </summary>
public sealed partial class SpawnInsideSlot : EventEntityEffect<SpawnInsideSlot>
{
    /// <summary>
    /// The inventory slot to check for a bag etc.
    /// </summary>
    [DataField(required: true)]
    public string Slot = string.Empty;

    [DataField(required: true)]
    public EntProtoId Entity;

    [DataField]
    public int Number = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
