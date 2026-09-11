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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CosmicTrailComponent : Component
{
    [DataField]
    public float CosmicFieldRadius = 0.5f;

    [DataField]
    public float CosmicFieldLifetime = 5f;

    [DataField]
    public int Strength;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextCosmicFieldTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan CosmicFieldPeriod = TimeSpan.FromSeconds(0.1f);
}
