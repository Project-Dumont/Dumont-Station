using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DnaClientComponent : Component
{
    [DataField, ViewVariables, AutoNetworkedField]
    public bool ConnectedToServer = false;

    [DataField, ViewVariables, AutoNetworkedField]
    public EntityUid? Server;
}
