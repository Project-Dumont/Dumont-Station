// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.StatusEffectNew.Components;

/// <summary>
/// Applies a set of permanent status effects while this component exists.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PermanentStatusEffectsComponent : Component
{
    /// <summary>
    /// The status effects to apply.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> StatusEffects = [];
}
