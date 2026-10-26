using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphActionAreaComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    [DataField, AutoNetworkedField] public bool TargetCircle;
    [DataField, AutoNetworkedField] public float TargetRange = 10;

    [DataField, AutoNetworkedField]
    public float Angle = 360;

    [DataField, AutoNetworkedField]
    public float Width;

    [DataField, AutoNetworkedField]
    public float Offset;

    [DataField, AutoNetworkedField]
    public bool GridTiles;

    [DataField, AutoNetworkedField]
    public bool SprayLine;

    [DataField, AutoNetworkedField]
    public bool TargetSquare;

    [DataField, AutoNetworkedField]
    public bool WideningSpray;
}
