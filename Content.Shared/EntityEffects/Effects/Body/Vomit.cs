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

using Content.Shared.Medical;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.Body;

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class Vomit : EntityEffectBase<Vomit>
{
    /// <summary>
    /// How much we adjust our thirst after vomiting.
    /// </summary>
    [DataField]
    public float ThirstAmount = -8f;

    /// <summary>
    /// How much we adjust our hunger after vomiting.
    /// </summary>
    [DataField]
    public float HungerAmount = -8f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-vomit", ("chance", Probability));
}
