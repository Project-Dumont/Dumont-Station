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

namespace Content.Trauma.Server.Heretic.Components.PathSpecific;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class HereticFlamesComponent : Component
{
    [DataField]
    public EntProtoId FireProto = "HereticFireAA";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan UpdateTimer = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LifetimeTimer = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateDuration = TimeSpan.FromMilliseconds(200);

    [DataField]
    public TimeSpan LifetimeDuration = TimeSpan.FromSeconds(60);

    [DataField]
    public int RangeIncrease;

    [DataField]
    public int Range = 1;
}
