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

using Content.Shared.Database;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.Atmos;

/// <summary>
/// See serverside system.
/// </summary>
/// <inheritdoc cref="EntityEffect"/>
public sealed partial class Flammable : EntityEffectBase<Flammable>
{
    /// <summary>
    /// Fire stack multiplier applied on an entity,
    /// unless that entity is already on fire and <see cref="MultiplierOnExisting"/> is not null.
    /// </summary>
    [DataField]
    public float Multiplier = 0.05f;

    /// <summary>
    /// Fire stack multiplier applied if the entity is already on fire. Defaults to <see cref="Multiplier"/> if null.
    /// </summary>
    [DataField]
    public float? MultiplierOnExisting;

    /// <summary>
    /// Goob - How much fire protection is mitigated (e.g. atmos firesuit+helmet is 1.0) when adding firestacks
    /// </summary>
    [DataField]
    public float FireProtectionPenetration;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-flammable-reaction",
            ("chance", Probability),
            ("direction", Multiplier < 0 ? "decrease" : "increase")); // Trauma - negative multiplier reduces flammability; MultiplierOnExisting's sign is not reflected

    public override LogImpact? Impact => LogImpact.Low;
}
