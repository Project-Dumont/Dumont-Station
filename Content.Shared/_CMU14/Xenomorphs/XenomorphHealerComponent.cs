using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphHealerComponent : Component
{
    [DataField, AutoNetworkedField] public int Transferred;
    [DataField, AutoNetworkedField] public int MaxTransferred = 7500;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan SalvingUntil;
    public int Displayed = -1;
}

public sealed partial class XenomorphSacrificeActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphHealingReserveAlertEvent : BaseAlertEvent;
