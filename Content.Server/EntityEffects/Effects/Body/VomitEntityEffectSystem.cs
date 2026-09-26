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

using Content.Server.Medical;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.Body;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.Body;

/// <summary>
/// Makes an entity vomit and reduces hunger and thirst by a given amount, modified by scale.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class VomitEntityEffectSystem : EntityEffectSystem<MetaDataComponent, Vomit>
{
    [Dependency] private VomitSystem _vomit = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<Vomit> args)
    {
        _vomit.Vomit(entity.Owner, args.Effect.ThirstAmount * args.Scale, args.Effect.HungerAmount * args.Scale);
    }
}
