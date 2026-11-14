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

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class ToggleAnimationComponent : Component
{
    [DataField]
    public TimeSpan ToggleOnTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan ToggleOffTime = TimeSpan.FromSeconds(1.6);

    [DataField, AutoNetworkedField]
    public ToggleAnimationState CurState;

    [DataField, AutoNetworkedField]
    public ToggleAnimationState NextState;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan ToggleEndTime = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan ToggleStartTime = TimeSpan.Zero;

    /// <summary>
    /// Whether this supports stopping toggling on/of animation and switching it to reverse
    /// when entity gets activated/deactivated
    /// </summary>
    [DataField]
    public bool ContinueReverseAnimation;
}

[Serializable, NetSerializable]
public enum ToggleAnimationVisuals : byte
{
    ToggleState,
}

[Serializable, NetSerializable, Flags]
public enum ToggleAnimationState : byte
{
    Off = 1,
    TogglingOn = 1 << 1,
    On = 1 << 2,
    TogglingOff = 1 << 3,
    None = 0,
    All = Off | TogglingOn | On | TogglingOff,
}
