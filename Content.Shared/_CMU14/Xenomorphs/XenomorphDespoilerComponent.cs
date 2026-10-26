using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphDespoilerComponent : Component
{
    [DataField, AutoNetworkedField] public int Stacks;
    [DataField] public float Points;
    [DataField, AutoNetworkedField] public bool Empowered;
    [DataField, AutoNetworkedField] public bool Armed;
    [DataField, AutoNetworkedField] public bool Charging;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan ChargeStartedAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastActivityAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan EmpowerExpiresAt;
}

public sealed partial class XenomorphAcidBarrageActionEvent : InstantActionEvent;
public sealed partial class XenomorphCausticEmbraceActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphOozingWoundsActionEvent : InstantActionEvent;
public sealed partial class XenomorphCatalyzeActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class XenomorphBarrageInputEvent(NetCoordinates target, bool firing) : EntityEventArgs
{
    public NetCoordinates Target = target;
    public bool Firing = firing;
}
