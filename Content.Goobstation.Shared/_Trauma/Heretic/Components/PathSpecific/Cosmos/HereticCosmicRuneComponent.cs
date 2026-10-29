// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class HereticCosmicRuneComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedRune;

    [DataField]
    public float Range = 0.75f;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/cosmic_energy.ogg");

    [DataField]
    public EntProtoId Effect = "HereticRuneCosmosLight";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan NextUse = TimeSpan.Zero;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);
}
