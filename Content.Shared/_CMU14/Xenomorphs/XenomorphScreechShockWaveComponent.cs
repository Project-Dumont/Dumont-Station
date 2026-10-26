using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphScreechShockWaveComponent : Component
{
    [DataField, AutoNetworkedField] public float WaveSpeed = 30.6f;
    [DataField, AutoNetworkedField] public float WaveStrength = 0.25f;
    [DataField, AutoNetworkedField] public float DownScale = 1;
}
