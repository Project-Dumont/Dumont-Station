// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Blade;
// Dumont end

namespace Content.Trauma.Shared.Heretic.EntityEffects;

public sealed partial class AddUserProtectiveBlade : EntityEffectBase<AddUserProtectiveBlade>;

public sealed partial class AddProtectiveBladeEffectSystem : EntityEffectSystem<TransformComponent, AddUserProtectiveBlade>
{
    [Dependency] private ProtectiveBladeSystem _pblade = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<AddUserProtectiveBlade> args)
    {
        if (args.User is not { } user)
            return;

        _pblade.AddProtectiveBlade(user, user);
    }
}
