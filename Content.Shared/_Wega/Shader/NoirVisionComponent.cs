using Robust.Shared.GameStates;

namespace Content.Shared.Shaders;

[RegisterComponent, NetworkedComponent]
public sealed partial class NoirVisionComponent : Component
{
    [DataField("redThreshold")]
    public float RedThreshold = 0.6f;

    [DataField("redSaturation")]
    public float RedSaturation = 3.0f;
}
