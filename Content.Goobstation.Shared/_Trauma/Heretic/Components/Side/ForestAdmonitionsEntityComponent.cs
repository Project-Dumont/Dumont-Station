// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ForestAdmonitionsEntityComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastRevealTime;

    [DataField, AutoNetworkedField]
    public float RevealDuration = 5f;

    [DataField, AutoNetworkedField]
    public float RevealDistance = 2f;

    [DataField, AutoNetworkedField]
    public float RevealDistanceSoft;

    [DataField, AutoNetworkedField]
    public float SelfVisibility = 0.2f;

    [DataField, AutoNetworkedField]
    public float ExamineThreshold = 0.2f;

    [DataField]
    public TimeSpan UpdateTime = TimeSpan.FromMilliseconds(100);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
