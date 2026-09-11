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

namespace Content.Shared.EntityEffects;

/// <summary>
/// Applies a set of EntityEffects to the entity upon map initialization.
/// <remarks>Useful for e.g. station spawning.</remarks>
/// </summary>
[RegisterComponent]
public sealed partial class EntityEffectOnMapInitComponent : Component
{
    /// <summary>
    /// Effects that should be applied upon the map being initiated.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects { get; set; } = default!;
}
