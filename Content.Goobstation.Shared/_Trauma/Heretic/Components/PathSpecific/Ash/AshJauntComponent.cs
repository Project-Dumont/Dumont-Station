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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class AshJauntComponent : Component
{
    [DataField]
    public EntProtoId OutEffect = "PolymorphAshJauntEndAnimation";

    [DataField]
    public TimeSpan EffectDuration = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan EndTime;

    [DataField]
    public bool SpawnedOutEffect;
}
