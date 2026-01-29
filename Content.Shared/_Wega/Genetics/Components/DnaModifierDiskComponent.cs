using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DnaModifierDiskComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EnzymeInfo? Data { get; set; }
}
