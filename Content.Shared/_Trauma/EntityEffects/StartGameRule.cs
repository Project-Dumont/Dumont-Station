// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Entity effect that starts a gamerule.
/// Target entity just has to exist, it doesn't have any impact on anything.
/// </summary>
public sealed partial class StartGameRule : EventEntityEffect<StartGameRule>
{
    [DataField(required: true)]
    public EntProtoId Rule;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
