using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphBoilerComponent : Component
{
    [DataField, AutoNetworkedField] public bool Neurotoxin;
    [DataField, AutoNetworkedField] public bool Preparing;
    public EntityUid? PreparingAction;
}

public sealed partial class XenomorphBombardActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphToggleGasActionEvent : InstantActionEvent;
public sealed partial class XenomorphAcidShroudActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphBombardDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Coordinates;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphAcidShroudDoAfterEvent : SimpleDoAfterEvent;
