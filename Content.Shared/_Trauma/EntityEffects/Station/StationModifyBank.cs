// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Cargo.Prototypes;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Station;

/// <summary>
/// Station effect that adds a random amount to a bank account's balance.
/// </summary>
public sealed partial class StationModifyBank : EventEntityEffect<StationModifyBank>
{
    /// <summary>
    /// The account to modify.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CargoAccountPrototype> Account;

    [DataField(required: true)]
    public int Min;

    [DataField(required: true)]
    public int Max;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
