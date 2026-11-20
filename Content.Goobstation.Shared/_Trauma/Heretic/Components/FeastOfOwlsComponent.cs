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

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class FeastOfOwlsComponent : Component
{
    [DataField]
    public int Reward = 5;

    [ViewVariables]
    public int CurrentStep;

    [DataField]
    public TimeSpan Timer = TimeSpan.FromSeconds(2);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JitterStutterTime = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier KnowledgeGainSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/eatfood.ogg");
}
