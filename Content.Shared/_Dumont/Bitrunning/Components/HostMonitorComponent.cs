using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Orion.Bitrunning.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HostMonitorComponent : Component
{
    [DataField, AutoNetworkedField]
    public HostMonitorMode Mode = HostMonitorMode.Integrity;
}

[Serializable, NetSerializable]
public enum HostMonitorMode : byte
{
    Integrity,
    Objective
}
