// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Shared.EntityConditions;

namespace Content.Goobstation.Shared.Religion.Nullrod;

public sealed partial class
    ProtectedByNullRodEntityConditionSystem : EntityConditionSystem<MetaDataComponent, ProtectedByNullRodCondition>
{
    [Dependency] private DivineInterventionSystem _divine = default!;

    protected override void Condition(Entity<MetaDataComponent> entity,
        ref EntityConditionEvent<ProtectedByNullRodCondition> args)
    {
        args.Result = _divine.TouchSpellDenied(entity);
    }
}

public sealed partial class ProtectedByNullRodCondition : EntityConditionBase<ProtectedByNullRodCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Inverted
            ? Loc.GetString("entity-condition-guidebook-nullrod-not-protected")
            : Loc.GetString("entity-condition-guidebook-nullrod-protected");
    }
}
