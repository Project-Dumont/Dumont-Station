using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphTunnelComponent : Component
{
    public const string Container = "xenomorph_tunnel";
    [DataField] public int MaxMobs = 3;
}

[Serializable, NetSerializable]
public enum XenomorphTunnelUiKey : byte { Destination, Rename }

[Serializable, NetSerializable]
public sealed class XenomorphTunnelUiState(string name, Dictionary<NetEntity, string> tunnels) : BoundUserInterfaceState
{
    public string Name = name;
    public Dictionary<NetEntity, string> Tunnels = tunnels;
}

[Serializable, NetSerializable]
public sealed class XenomorphTunnelTravelMessage(NetEntity destination) : BoundUserInterfaceMessage
{
    public NetEntity Destination = destination;
}

[Serializable, NetSerializable]
public sealed class XenomorphTunnelNameMessage(string name) : BoundUserInterfaceMessage
{
    public string Name = name;
}

[Serializable, NetSerializable]
public sealed class XenomorphTunnelExitMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed partial class XenomorphTunnelEnterDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphTunnelTravelDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetEntity Destination;
}
