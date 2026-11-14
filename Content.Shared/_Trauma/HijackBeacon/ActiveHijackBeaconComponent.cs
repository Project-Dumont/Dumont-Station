// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.HijackBeacon;

/// <summary>
/// This is used for tracking a <see cref="HijackBeaconComponent"/> that is currently activated or on cooldown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ActiveHijackBeaconComponent : Component
{
    /// <summary>
    ///     Remaining time until the hijack is completed.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan CompletionTime = TimeSpan.Zero;
}
