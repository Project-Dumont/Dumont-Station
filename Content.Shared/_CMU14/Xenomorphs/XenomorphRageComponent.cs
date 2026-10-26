using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphRageComponent : Component
{
    [DataField, AutoNetworkedField] public int Rage;
    [DataField, AutoNetworkedField] public bool Locked;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastHit;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LockUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan CooldownUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan HealAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan ApprehendUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan AuraUntil;
    [DataField, AutoNetworkedField] public Color AuraColor;
    [DataField, AutoNetworkedField] public float AuraWidth = 3;
    public bool ApprehendHit;
    public float PendingHeal;
}

public sealed partial class XenomorphApprehendActionEvent : InstantActionEvent;
public sealed partial class XenomorphClotheslineActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphEviscerateActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphEviscerateDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public int Rage;
}

[RegisterComponent]
public sealed partial class XenomorphDazeableActionComponent : Component;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphDazedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan Until;
    public HashSet<EntityUid> DisabledActions = new();
}
