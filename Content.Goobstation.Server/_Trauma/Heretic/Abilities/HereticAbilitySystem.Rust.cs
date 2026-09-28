// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Flash;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;
// Dumont end

namespace Content.Trauma.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    [SubscribeLocalEvent]
    private void OnFlashAttempt(Entity<RustbringerComponent> ent, ref FlashAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancelled = true;
    }
}
