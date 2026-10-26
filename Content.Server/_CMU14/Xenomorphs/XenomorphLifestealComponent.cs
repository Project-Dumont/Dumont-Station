using Content.Server._Dumont.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphLifestealComponent : Component
{
    [DataField]
    public FixedPoint2 BasePercentage = 0.07;

    [DataField]
    public FixedPoint2 MaxPercentage = 0.09;

    [DataField]
    public FixedPoint2 TargetIncreasePercentage = 0.01;

    [DataField]
    public FixedPoint2 MinHeal = 20;

    [DataField]
    public FixedPoint2 MaxHeal = 40;

    [DataField]
    public float TargetRange = 3;
}
