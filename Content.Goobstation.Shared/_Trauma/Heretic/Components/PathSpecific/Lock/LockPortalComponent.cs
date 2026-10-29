// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class LockPortalComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedPortal;

    [DataField, AutoNetworkedField]
    public bool Inverted;

    [DataField]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
