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

using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Washes the cream pie off of this entity face.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
/// TODO: This can probably be made into a generic "CleanEntityEffect" which multiple components listen to...
public sealed partial class WashCreamPieEntityEffectSystem : EntityEffectSystem<CreamPiedComponent, WashCreamPie>
{
    [Dependency] private SharedCreamPieSystem _creamPie = default!;

    protected override void Effect(Entity<CreamPiedComponent> entity, ref EntityEffectEvent<WashCreamPie> args)
    {
        _creamPie.SetCreamPied(entity, entity.Comp, false);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class WashCreamPie : EntityEffectBase<WashCreamPie>
{
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-wash-cream-pie-reaction", ("chance", Probability));
}
