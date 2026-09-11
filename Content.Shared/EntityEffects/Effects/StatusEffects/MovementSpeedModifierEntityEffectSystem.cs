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

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects.StatusEffects;

/// <summary>
/// Applies a given movement speed modifier status effect to this entity.
/// Duration is modified by scale.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class MovementSpeedModifierEntityEffectSystem : EntityEffectSystem<MovementSpeedModifierComponent, MovementSpeedModifier>
{
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private MovementModStatusSystem _movementModStatus = default!;

    protected override void Effect(Entity<MovementSpeedModifierComponent> entity, ref EntityEffectEvent<MovementSpeedModifier> args)
    {
        var proto = args.Effect.EffectProto;
        var sprintMod = args.Effect.SprintSpeedModifier;
        var walkMod = args.Effect.WalkSpeedModifier;

        switch (args.Effect.Type)
        {
            case StatusEffectMetabolismType.Update:
                _movementModStatus.TryUpdateMovementSpeedModDuration(
                    entity,
                    proto,
                    args.Effect.Time * args.Scale,
                    sprintMod,
                    walkMod);
                break;
            case StatusEffectMetabolismType.Add:
                if (args.Effect.Time != null)
                {
                    _movementModStatus.TryAddMovementSpeedModDuration(
                        entity,
                        proto,
                        args.Effect.Time.Value * args.Scale,
                        sprintMod,
                        walkMod);
                }
                else
                {
                    _movementModStatus.TryUpdateMovementSpeedModDuration(
                        entity,
                        proto,
                        args.Effect.Time * args.Scale,
                        sprintMod,
                        walkMod);
                }
                break;
            case StatusEffectMetabolismType.Remove:
                _status.TryRemoveTime(entity, args.Effect.EffectProto, args.Effect.Time * args.Scale);
                break;
            case StatusEffectMetabolismType.Set:
                _status.TrySetStatusEffectDuration(entity, proto, args.Effect.Time * args.Scale);
                break;
        }
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class MovementSpeedModifier : BaseStatusEntityEffect<MovementSpeedModifier>
{
    /// <summary>
    /// How much the entities' walk speed is multiplied by.
    /// </summary>
    [DataField]
    public float WalkSpeedModifier = 1f;

    /// <summary>
    /// How much the entities' run speed is multiplied by.
    /// </summary>
    [DataField]
    public float SprintSpeedModifier = 1f;

    /// <summary>
    /// Movement speed modifier prototype we're adding. Adding in case we ever want more than one prototype that boosts speed.
    /// </summary>
    [DataField]
    public EntProtoId EffectProto = new EntProtoId("ReagentSpeedStatusEffect");

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
    Time == null
        ? null // Not gonna make a whole new looc for something that shouldn't ever exist.
        : Loc.GetString("entity-effect-guidebook-movespeed-modifier",
            ("chance", Probability),
            ("sprintspeed", SprintSpeedModifier),
            ("time", Time.Value.TotalSeconds));
}
