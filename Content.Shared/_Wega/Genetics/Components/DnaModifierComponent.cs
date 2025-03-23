using Content.Shared.Genetics.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedDnaModifierSystem))]
public sealed partial class DnaModifierComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField("uniqueIdentifiers"), AutoNetworkedField]
    public UniqueIdentifiersPrototype? UniqueIdentifiers { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public List<EnzymesPrototypeInfo>? EnzymesPrototypes { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly), DataField("instability")]
    public int Instability { get; set; } = 0;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string Upper = string.Empty;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string Lowest = string.Empty;
}
