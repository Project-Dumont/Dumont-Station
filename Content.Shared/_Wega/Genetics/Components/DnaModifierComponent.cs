using Content.Shared.Genetics.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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

    [ValidatePrototypeId<EntityPrototype>, ViewVariables(VVAccess.ReadOnly), DataField]
    public string Upper = string.Empty;

    [ValidatePrototypeId<EntityPrototype>, ViewVariables(VVAccess.ReadOnly), DataField]
    public string Lowest = string.Empty;
}
