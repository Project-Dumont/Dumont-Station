// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Adds components to the target entity.
/// </summary>
public sealed partial class AddComponents : EntityEffectBase<AddComponents>
{
    /// <summary>
    /// Components to add.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components;

    /// <summary>
    /// Text to use for the guidebook entry for reagents.
    /// </summary>
    [DataField]
    public LocId? GuidebookText;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => GuidebookText is {} loc ? Loc.GetString(loc, ("chance", Probability)) : null;
}

public sealed class AddComponentsEffectSystem : EntityEffectSystem<MetaDataComponent, AddComponents>
{
    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<AddComponents> args)
    {
        EntityManager.AddComponents(ent, args.Effect.Components);
    }
}
