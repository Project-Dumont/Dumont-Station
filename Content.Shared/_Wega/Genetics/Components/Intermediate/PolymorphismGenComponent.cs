using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class PolymorphismGenComponent : Component
{
    public readonly EntProtoId PolymorphismAction = "ActionGenPolymorphism";

    public EntityUid? PolymorphismActionEntity { get; set; }
}
