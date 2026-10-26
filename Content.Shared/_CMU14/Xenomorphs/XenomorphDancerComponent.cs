using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphDancerComponent : Component
{
    public int ProjectileHits;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextYellowSpread;
    [DataField, AutoNetworkedField] public bool Dodging;
    [DataField, AutoNetworkedField] public bool InCrowd;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan DodgeUntil;
}

public sealed partial class XenomorphDodgeActionEvent : InstantActionEvent;

public sealed partial class XenomorphImpaleActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphTailTripActionEvent : EntityTargetActionEvent;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphDancerMarkComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan RedUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan YellowUntil;
}
