// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Server.Atmos.EntitySystems;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Ash;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems.PathSpecific;

public sealed partial class ScorchedMantleSystem : SharedScorchedMantleSystem
{
    [Dependency] private FlammableSystem _flammable = default!;

    // Dumont start
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash.ScorchedMantleComponent>();
        while (query.MoveNext(out var mantle))
        {
            if (!TryComp(mantle.Action, out Content.Shared.Actions.Components.ActionComponent? action) ||
                !action.Toggled || action.AttachedEntity is not { } wearer ||
                !TryComp(wearer, out Content.Shared.Atmos.Components.FlammableComponent? flammable) ||
                flammable.OnFire || flammable.Resisting)
                continue;

            var ev = new Content.Trauma.Common.Heretic.NoFirestacksUpdateEvent(wearer);
            RaiseLocalEvent(wearer, ref ev);
        }
    }
    // Dumont end

    protected override void UpdateFirestacks(EntityUid uid)
    {
        base.UpdateFirestacks(uid);

        _flammable.SetFireStacks(uid, 0.1f, ignite: true);
    }
}
