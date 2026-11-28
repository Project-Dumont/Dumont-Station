// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ES.Sabotage;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class HeavyGravitySabotageComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ActivatesAt;

    [DataField]
    public EntityUid Station;

    [DataField]
    public bool Announced;
}
