using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Shaders;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ColourBlindnessComponent : Component
{
    [DataField("colorFilter"), AutoNetworkedField]
    public Vector3 ColorFilter = new Vector3(1.0f, 1.0f, 1.0f);

    [DataField("desaturation"), AutoNetworkedField]
    public float Desaturation = 1.0f;

    [DataField("colorShift"), AutoNetworkedField]
    public Vector3 ColorShift = Vector3.Zero;
}
