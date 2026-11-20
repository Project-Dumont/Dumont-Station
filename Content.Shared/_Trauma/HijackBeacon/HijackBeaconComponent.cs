// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared.HijackBeacon;

/// <summary>
///     Component for hijack beacons, meant to be planted on the ATS to drain station funds.
/// </summary>
/// <remarks>
///     Status and timer fields are private so the state machine is preserved.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class HijackBeaconComponent : Component
{
    /// <summary>
    ///     Current state of the beacon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HijackBeaconStatus Status = HijackBeaconStatus.AwaitActivate;

    /// <summary>
    ///     How long it takes to deactivate the beacon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DeactivationLength = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Remaining time until the hijack is completed.
    /// </summary>
    [DataField]
    public TimeSpan RemainingTime = TimeSpan.FromSeconds(600); // Trauma, should be difficult to defend, was 200

    /// <summary>
    ///     Default amount of time before the beacon can be re-activated, if it is disarmed.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(20);

    /// <summary>
    ///     Remaining cooldown time before the beacon can be reactivated.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan CooldownTime = TimeSpan.Zero;

    /// <summary>
    ///     How much cash should be withdrawn from each department account?
    /// </summary>
    [DataField]
    public int Fine = 15000; // Trauma, was 5000
}


public enum HijackBeaconStatus : byte
{
    AwaitActivate,
    Armed,
    Cooldown,
    HijackComplete
}

/// <summary>
///     DoAfter event raised when the hijack beacon is deactivated.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class HijackBeaconDeactivateDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
///     Event raised when the hijack beacon succeeds in hijacking the ATS.
/// </summary>
[ByRefEvent]
public record struct HijackBeaconSuccessEvent(int Fine)
{
    public int Total = 0;
};
