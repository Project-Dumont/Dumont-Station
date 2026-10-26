using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphBurrowComponent : Component
{
    public HashSet<EntityUid> DisabledActions = new();
    [DataField, AutoNetworkedField] public bool Active;
    [DataField, AutoNetworkedField] public bool Tunneling;
    [DataField, AutoNetworkedField] public bool BuildingTunnel;
    [DataField] public float MaxTunnelingDistance = 15;
    [DataField] public TimeSpan BurrowLength = TimeSpan.FromSeconds(1.5);
    [DataField] public TimeSpan BurrowCooldown = TimeSpan.FromSeconds(2);
    [DataField] public TimeSpan TunnelCooldown = TimeSpan.FromSeconds(7);
    [DataField] public TimeSpan BurrowMaxDuration = TimeSpan.FromSeconds(9);
    [DataField] public float UnburrowStunRange = 0.5f;
    [DataField] public TimeSpan UnburrowStunLength = TimeSpan.FromSeconds(3);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? NextTunnelAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? NextBurrowAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? ForcedUnburrowAt;
}

public sealed partial class XenomorphBurrowActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphTremorActionEvent : InstantActionEvent;
public sealed partial class XenomorphDigTunnelActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphBurrowDownDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphBurrowMoveDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Destination;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphDigTunnelDoAfterEvent : SimpleDoAfterEvent;
