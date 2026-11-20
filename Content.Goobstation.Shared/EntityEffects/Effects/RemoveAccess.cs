// SPDX-License-Identifier: AGPL-3.0-or-later

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
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.EntityEffects.Effects;

/// <summary>
/// Removes access provided by the target entity ID card.
/// If the target entity is a mob or PDA it will look for a PDA or ID in its hands or ID slot instead.
/// </summary>
public sealed partial class RemoveAccess : EntityEffectBase<RemoveAccess>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Removes all target access.";
}

public sealed partial class RemoveAccessEffectSystem : EntityEffectSystem<TransformComponent, RemoveAccess>
{
    [Dependency] private SharedAccessSystem _access = default!;
    [Dependency] private SharedIdCardSystem _idCard = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<RemoveAccess> args)
    {
        if (!_idCard.TryFindIdCard(ent, out var id))
            return;

        _access.TrySetTags(id, new List<ProtoId<AccessLevelPrototype>>());
    }
}
