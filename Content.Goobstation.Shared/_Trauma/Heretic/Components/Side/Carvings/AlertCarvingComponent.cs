// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Whitelist;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side.Carvings;

[RegisterComponent, NetworkedComponent]
public sealed partial class AlertCarvingComponent : Component
{
    [DataField]
    public EntityUid? User;

    [DataField]
    public SoundSpecifier? AlertSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/curse.ogg");

    [DataField]
    public int TeleportDelay = 5000;
}
