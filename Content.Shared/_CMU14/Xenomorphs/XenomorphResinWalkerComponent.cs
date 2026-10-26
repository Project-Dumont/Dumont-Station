using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphResinWalkerComponent : Component
{
    [DataField, AutoNetworkedField] public bool Active;
    [DataField, AutoNetworkedField] public bool OnWeeds;
    [DataField] public float SpeedMultiplier = 1.66f;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextDrain;
}

public sealed partial class XenomorphResinWalkerActionEvent : InstantActionEvent;
public sealed partial class XenomorphHivelordPlasmaActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphHivelordPlasmaDoAfterEvent : SimpleDoAfterEvent;
