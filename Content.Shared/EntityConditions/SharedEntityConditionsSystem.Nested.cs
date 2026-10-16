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
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions;

/// <summary>
/// Entity condition API counterpart using <see cref="EntityConditionPrototype"/> instead of <see cref="EntityCondition"/>.
/// </summary>
public sealed partial class SharedEntityConditionsSystem
{
    /// <summary>
    /// <c>TryCondition</c> overload that uses a <see cref="EntityConditionPrototype"/> instead of <see cref="EntityCondition"/>.
    /// </summary>
    public bool TryCondition(EntityUid target, [ForbidLiteral] ProtoId<EntityConditionPrototype> id,
        EntityUid? sourceEnt = null) // Trauma
    {
        var proto = ProtoMan.Index(id);
        return TryCondition(target, proto.Condition, sourceEnt: sourceEnt); // Trauma - pass sourceEnt
    }
}
