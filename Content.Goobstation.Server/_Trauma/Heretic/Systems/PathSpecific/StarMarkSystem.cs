// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Cosmos;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems.PathSpecific;

public sealed partial class StarMarkSystem : SharedStarMarkSystem
{
    [Dependency] private AirtightSystem _airtight = default!;

    protected override void InitializeCosmicField(Entity<CosmicFieldComponent> field, int strength)
    {
        base.InitializeCosmicField(field, strength);

        if (strength < 2)
            return;

        var airtight = EnsureComp<AirtightComponent>(field);
        airtight.BlockExplosions = true;
        _airtight.UpdatePosition((field.Owner, airtight));
    }
}
