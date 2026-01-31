using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DizzyGenComponent : Component
{
    [DataField]
    public float InitialIntensity = 200f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class DizzyEffectComponent : Component
{
    [DataField]
    public float Intensity = 0f;
}
