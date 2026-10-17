// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Spawn any items from an entity table and try to equip or pick up them to the target entity.
/// </summary>
public sealed partial class EquipEntityTable : EventEntityEffect<EquipEntityTable>
{
    [DataField(required: true)]
    public string Slot = string.Empty;

    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    [DataField]
    public bool Silent;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
