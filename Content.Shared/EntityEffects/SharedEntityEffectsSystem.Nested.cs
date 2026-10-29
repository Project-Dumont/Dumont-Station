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

using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Shared.EntityEffects;

/// <summary>
/// Entity effects API counterparts using <see cref="EntityEffectPrototype"/> instead of <see cref="EntityEffect"/>.
/// </summary>
public sealed partial class SharedEntityEffectsSystem
{
    /// <summary>
    /// <c>TryApplyEffect</c> overload using a <see cref="EntityEffectPrototype"/> instead of <see cref="EntityEffect"/>.
    /// </summary>
    public void TryApplyEffect(EntityUid target, [ForbidLiteral] ProtoId<EntityEffectPrototype> id, float scale = 1f, EntityUid? user = null,
        bool predicted = true) // Trauma
    {
        var proto = ProtoMan.Index(id);
        if (TryConditions(target, proto.Conditions, user))
            ApplyEffects(target, proto.Effects, scale, user,
                predicted: predicted); // Trauma
    }
}
