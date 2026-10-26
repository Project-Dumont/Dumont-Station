using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphSweepingComponent : Component
{
    [DataField, AutoNetworkedField] public Direction? LastDirection;
    [DataField] public TimeSpan Delay = TimeSpan.FromSeconds(0.07);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextRotation;
    [DataField] public int TotalRotations;
}
