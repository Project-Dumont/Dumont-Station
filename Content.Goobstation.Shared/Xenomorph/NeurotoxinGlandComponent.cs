using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenomorph;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NeurotoxinGlandComponent : Component
{
    // Dumont start
    [DataField, AutoNetworkedField]
    public bool Active = true;
    // Dumont end
}
