// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared._Starlight.CollectiveMind;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Events;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    [SubscribeLocalEvent]
    private void OnAscensionLock(HereticAscensionLockEvent args)
    {
        var collectiveMind = EnsureComp<CollectiveMindComponent>(args.Heretic);
        if (args.Negative)
            collectiveMind.Channels.Remove(MansusLinkMind);
        else
            collectiveMind.Channels.Add(MansusLinkMind);
    }
}
