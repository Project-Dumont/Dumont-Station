using Robust.Shared.Audio;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class MatterEaterGenComponent : Component
{
    [DataField("eatDelay")]
    public float EatDelay = 3f;

    [DataField("sound")]
    public SoundSpecifier? EatSound = new SoundCollectionSpecifier("eating");
}
