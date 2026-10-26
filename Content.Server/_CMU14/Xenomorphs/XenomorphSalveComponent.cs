using Content.Server._Dumont.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphSalveComponent : Component
{
    [DataField]
    public List<XenomorphSalveStack> Stacks = new();
}

[DataDefinition]
public sealed partial class XenomorphSalveStack
{
    [DataField]
    public FixedPoint2 Amount;

    [DataField]
    public int Charges = 5;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextHeal;
}
