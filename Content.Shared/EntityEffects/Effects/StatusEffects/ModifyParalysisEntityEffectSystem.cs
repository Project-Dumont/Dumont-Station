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

using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.StatusEffects;

/// <summary>
/// Applies the paralysis status effect to this entity.
/// Duration is modified by scale.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class ModifyParalysisEntityEffectSystem : EntityEffectSystem<MetaDataComponent, ModifyParalysis>
{
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<ModifyParalysis> args)
    {
        var time = args.Effect.Time * args.Scale;

        switch (args.Effect.Type)
        {
            case StatusEffectMetabolismType.Update:
                _stun.TryUpdateParalyzeDuration(entity, time);
                break;
            case StatusEffectMetabolismType.Add:
                if (time is { } duration)
                    _stun.TryAddParalyzeDuration(entity, duration);
                else
                    _stun.TryUpdateParalyzeDuration(entity, null);
                break;
            case StatusEffectMetabolismType.Remove:
                _status.TryRemoveTime(entity, SharedStunSystem.StunId, time);
                break;
            case StatusEffectMetabolismType.Set:
                _status.TrySetStatusEffectDuration(entity, SharedStunSystem.StunId, time);
                break;
        }
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class ModifyParalysis : BaseStatusEntityEffect<ModifyParalysis>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Time == null
            ? null // Not gonna make a whole new looc for something that shouldn't ever exist.
            : Loc.GetString(
            "entity-effect-guidebook-paralyze",
            ("chance", Probability),
            ("time", Time.Value.TotalSeconds)
        );
}
