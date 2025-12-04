using Content.Shared.Genetics.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedDnaModifierSystem))]
public sealed partial class DnaModifierComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField("uniqueIdentifiers"), AutoNetworkedField]
    public UniqueIdentifiersData? UniqueIdentifiers { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public List<EnzymesPrototypeInfo>? EnzymesPrototypes { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly), DataField("instability")]
    public int Instability { get; set; } = 0;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntProtoId? Upper = default!;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntProtoId? Lowest = default!;
}
