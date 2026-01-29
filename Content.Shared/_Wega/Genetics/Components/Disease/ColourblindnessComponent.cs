using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class ColourBlindnessComponent : Component
{
    [DataField("desaturationAmount")]
    public float DesaturationAmount = 1.0f;
}
