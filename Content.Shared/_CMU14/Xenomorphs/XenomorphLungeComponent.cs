using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphLungeComponent : Component
{
    [DataField] public float Range = 5;
    [DataField] public float Speed = 30;
    [DataField] public TimeSpan StunTime = TimeSpan.FromSeconds(4);
    public EntityUid? Target;
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphLungeStunnedComponent : Component
{
    public EntityUid Stunner;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExpiresAt;
}

public sealed partial class XenomorphLungeActionEvent : EntityTargetActionEvent;
