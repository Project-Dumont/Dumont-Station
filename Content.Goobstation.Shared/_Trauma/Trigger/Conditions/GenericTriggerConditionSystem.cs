// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.EntityConditions;
using Content.Shared.Trigger;

namespace Content.Trauma.Shared.Trigger.Conditions;

public sealed partial class GenericTriggerConditionSystem : EntitySystem
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenericTriggerConditionComponent, AttemptTriggerEvent>(OnAttemptTrigger);
    }

    private void OnAttemptTrigger(Entity<GenericTriggerConditionComponent> ent, ref AttemptTriggerEvent args)
    {
        if (args.Key is not {} key || !ent.Comp.Keys.Contains(key))
            return;

        if ((ent.Comp.CheckUser ? args.User : ent.Owner) is not {} target)
        {
            args.Cancelled = true;
            return;
        }

        args.Cancelled |= !_conditions.TryCondition(target, ent.Comp.Condition, args.User);
    }
}
