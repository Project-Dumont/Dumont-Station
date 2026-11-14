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
using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.EntityConditions;

public sealed partial class HasStatusEffectCondition : EntityConditionBase<HasStatusEffectCondition>
{
    [DataField(required: true)]
    public EntProtoId EffectProto;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
        => Loc.GetString("reagent-effect-guidebook-has-status-effect",
            ("effect", prototype.Index(EffectProto).Name),
            ("invert", Inverted));
}

public sealed partial class HasStatusEffectConditionSystem : EntityConditionSystem<MetaDataComponent, HasStatusEffectCondition>
{
    [Dependency] private StatusEffectsSystem _status = default!;

    protected override void Condition(Entity<MetaDataComponent> entity,
        ref EntityConditionEvent<HasStatusEffectCondition> args)
    {
        args.Result = _status.HasStatusEffect(entity.Owner, args.Condition.EffectProto);
    }
}
