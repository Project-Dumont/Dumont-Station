// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityConditions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.EffectConditions;

/// <summary>
/// Returns true if this entity's current mob state matches the condition's specified mob state.
/// </summary>
/// <inheritdoc cref="EntityConditionSystem{T, TCondition}"/>
public sealed partial class MobStateEntityConditionSystem : EntityConditionSystem<MobStateComponent, MobStateCondition>
{
    protected override void Condition(Entity<MobStateComponent> entity, ref EntityConditionEvent<MobStateCondition> args)
    {
        if (entity.Comp.CurrentState == args.Condition.Mobstate)
            args.Result = true;
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class MobStateCondition : EntityConditionBase<MobStateCondition>
{
    /// <summary>
    /// The mobstate necessary to fulfill this condition.
    /// </summary>
    [DataField]
    public MobState Mobstate = MobState.Alive;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
        Loc.GetString("entity-condition-guidebook-mob-state-condition", ("state", Mobstate));
}
