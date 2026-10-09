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

using Content.Shared.EntityConditions.Conditions;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions;

/// <summary>
/// A prototype for entity conditions which can be reused via <see cref="NestedCondition"/>.
/// </summary>
[Prototype]
public sealed partial class EntityConditionPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// The condition of this prototype.
    /// </summary>
    [DataField(required: true)]
    public EntityCondition Condition = default!;
}
