using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DnaServerComponent : Component
{
    [DataField, ViewVariables, AutoNetworkedField]
    public int ServerId;

    [ViewVariables]
    public HashSet<EntityUid> Clients = [];

    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EnzymeInfo? Buffer1 { get; set; }

    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EnzymeInfo? Buffer2 { get; set; }

    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EnzymeInfo? Buffer3 { get; set; }
}
