using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class PolymorphismGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string PolymorphismAction = "ActionGenPolymorphism";

    public EntityUid? PolymorphismActionEntity { get; set; }
}
