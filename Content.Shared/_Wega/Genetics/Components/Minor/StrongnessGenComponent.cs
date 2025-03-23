using Content.Shared.FixedPoint;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class StrongnessGenComponent : Component
{
    [DataField]
    public FixedPoint2 OldDamage = 0;

    [DataField]
    public FixedPoint2 StrongnessModifier = 6;
}
