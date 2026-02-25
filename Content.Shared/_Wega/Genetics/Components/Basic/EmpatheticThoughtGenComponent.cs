namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class EmpatheticThoughtGenComponent : Component
{
    [DataField("range")]
    public float Range = 3f;

    [DataField("minInterval")]
    public float MinInterval = 20f;

    [DataField("maxInterval")]
    public float MaxInterval = 30f;

    public float NextTimeTick;
}
