using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphHedgehogComponent : Component
{
    [DataField, AutoNetworkedField] public int Shards;
    [DataField, AutoNetworkedField] public int MaxShards = 300;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan ShedUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextGain;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ShieldUntil;
    [DataField] public float Shield;
    public EntityUid? ShieldEffect;
    public EntityUid? IncomingShooter;
    public TimeSpan IncomingAt;
    public bool IncomingProjectile;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphSpikeShieldVisualComponent : Component;

public sealed partial class XenomorphFireSpikesActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphSpikeShieldActionEvent : InstantActionEvent;
public sealed partial class XenomorphSpikeShedActionEvent : InstantActionEvent;
public sealed partial class XenomorphShardsAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public enum XenomorphHedgehogVisuals : byte { ShieldFraction }
