using Content.Shared.FixedPoint;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class RegenerationGenComponent : Component
{
    public FixedPoint2 RegenerationModifier = -0.5;

    public float NextTimeTick;
}
