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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Void;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class HollowWeaveComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan NextStatus;

    [DataField]
    public TimeSpan StatusDelay = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan StatusDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public EntProtoId StatusEffect = "VoidCamouflageStatusEffect";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/bamf.ogg");
}
