using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphResinWhispererComponent : Component
{
    [DataField] public float Range = 11;
    [DataField] public float RemoteDelayMultiplier = 2.5f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphResinDoorComponent : Component;

public sealed partial class XenomorphRemoteResinActionEvent : WorldTargetActionEvent
{
    [DataField] public bool Door;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphRemoteResinDoAfterEvent : SimpleDoAfterEvent
{
    public NetCoordinates Coordinates;
    public bool Door;
}
