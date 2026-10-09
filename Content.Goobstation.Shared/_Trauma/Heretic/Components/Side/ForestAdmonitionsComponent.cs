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

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class ForestAdmonitionsComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public EntProtoId CloakEntity = "ShadowCloakEntityPale";

    [DataField]
    public EntProtoId FogProto = "HereticPaleFog";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromMilliseconds(250);

    [DataField]
    public int Range = 5;

    [DataField]
    public float FogSlope = 4f;
}
