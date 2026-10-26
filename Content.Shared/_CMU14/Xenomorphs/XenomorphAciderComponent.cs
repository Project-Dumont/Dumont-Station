using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphAciderComponent : Component
{
    [DataField, AutoNetworkedField] public int Acid;
    [DataField, AutoNetworkedField] public bool Primed;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextGain;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan GeneratingUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan DetonateAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextCountdown;
    public EntityUid? PrimeAction;
}

public sealed partial class XenomorphPrimeAcidActionEvent : InstantActionEvent;
public sealed partial class XenomorphHideActionEvent : InstantActionEvent;
public sealed partial class XenomorphAciderCorrodeActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphAcidReserveAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphAciderCorrodeDoAfterEvent : SimpleDoAfterEvent;
