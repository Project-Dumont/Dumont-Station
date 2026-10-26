using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphVanguardComponent : Component
{
    [DataField, AutoNetworkedField] public float Shield;
    [DataField, AutoNetworkedField] public bool BlitzActive;
    public bool ShieldHit;
    public bool SlashReady;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextDecay;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastHit;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastRecharge;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan BlitzStarted;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan SlashAt;
}

public sealed partial class XenomorphPierceActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphCleaveActionEvent : EntityTargetActionEvent
{
    [DataField] public bool Flings;
}

[ByRefEvent]
public readonly record struct XenomorphLeapStartedEvent;
