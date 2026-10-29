// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.EntityConditions;

namespace Content.Medical.Shared.EntityConditions;

/// <summary>
/// Condition that checks if the target mob has at least 1 organ of a given category.
/// Since it uses organ categories, this is symmetry sensitive.
/// Optionally can check nested entity conditions against the organ.
/// </summary>
public sealed partial class HasOrgan : EntityConditionBase<HasOrgan>
{
    [DataField(required: true)]
    public string OrganCategory = string.Empty;

    /// <summary>
    /// Optional conditions the organ must meet.
    /// </summary>
    [DataField]
    public EntityCondition[]? Conditions;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
        => Loc.GetString("entity-condition-guidebook-has-organ",
            ("invert", Inverted),
            ("organ", OrganCategory));
}

public sealed partial class HasOrganConditionSystem : EntityConditionSystem<BodyComponent, HasOrgan>
{
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    protected override void Condition(Entity<BodyComponent> entity, ref EntityConditionEvent<HasOrgan> args)
    {
        foreach (var part in _body.GetBodyChildren(entity.Owner, entity.Comp))
        {
            if (string.Equals(part.Component.PartType.ToString(), args.Condition.OrganCategory, StringComparison.OrdinalIgnoreCase)
                && _conditions.TryConditions(part.Id, args.Condition.Conditions, args.SourceEnt))
            {
                args.Result = true;
                return;
            }
        }

        foreach (var organ in _body.GetBodyOrgans(entity.Owner, entity.Comp))
        {
            if (string.Equals(organ.Component.SlotId, args.Condition.OrganCategory, StringComparison.OrdinalIgnoreCase)
                && _conditions.TryConditions(organ.Id, args.Condition.Conditions, args.SourceEnt))
            {
                args.Result = true;
                return;
            }
        }

        args.Result = false;
    }
}
