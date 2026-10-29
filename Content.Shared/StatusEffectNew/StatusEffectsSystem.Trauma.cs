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

namespace Content.Shared.StatusEffectNew;

public sealed partial class StatusEffectsSystem
{
    /// <summary>
    /// Add a permanent status effect to an entity
    /// </summary>
    public void AddEffect(EntityUid target, [ForbidLiteral] EntProtoId id)
    {
        TryAddStatusEffect(target, id, out _);
    }

    /// <summary>
    /// Add a list of permanent status effects to an entity
    /// </summary>
    public void AddEffects(EntityUid target, IReadOnlyList<EntProtoId> effects)
    {
        foreach (var id in effects)
        {
            AddEffect(target, id);
        }
    }

    public void RemoveEffects(EntityUid target, IReadOnlyList<EntProtoId> effects)
    {
        foreach (var id in effects)
        {
            TryRemoveStatusEffect(target, id);
        }
    }
}
