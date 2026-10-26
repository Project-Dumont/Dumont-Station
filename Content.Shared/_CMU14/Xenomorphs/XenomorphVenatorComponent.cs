using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphVenatorComponent : Component
{
    [DataField, AutoNetworkedField] public int AcidCharges;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan StoreLockedUntil;
}

public sealed partial class XenomorphStoreAcidActionEvent : InstantActionEvent;
public sealed partial class XenomorphVenatorSpitActionEvent : WorldTargetActionEvent
{
    [DataField] public bool Blob;
    [DataField] public bool Breath;
}
