using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphSpitterComponent : Component
{
    [DataField, AutoNetworkedField] public bool Charging;
    [DataField] public bool Fired;
    [DataField] public bool StrongSpray;
    [DataField] public bool StackingSpray;
    [DataField] public float SprayRange = 6;
    [DataField] public int SprayCost = 40;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExpiresAt;
}

public sealed partial class XenomorphChargeSpitActionEvent : InstantActionEvent;
public sealed partial class XenomorphSprayAcidActionEvent : WorldTargetActionEvent;
