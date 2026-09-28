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
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components.Effects;

/// <summary>
/// Applies a list of entity effects to the owning entity when triggered.
/// If TargetUser is true then they will be applied to the user instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EntityEffectOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The effects to apply.
    /// </summary>
    [DataField] // Trauma - unnetworked because EntityEffect isnt serializable, chud major
    public EntityEffect[] Effects;

    /// <summary>
    /// Optional scale multiplier for the effects.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Scale = 1f;
}
