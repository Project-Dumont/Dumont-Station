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
public sealed partial class MadnessMaskComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;

    [DataField]
    public float MaxRange = 8f;

    [DataField]
    public float DistFearModifier = 1.5f;

    [DataField]
    public float ViewFearModifier = 2f;

    [DataField]
    public float MaxFear = 5f;

    [DataField]
    public TimeSpan NonHereticToggleFlahsDuration = TimeSpan.FromSeconds(10);
}
