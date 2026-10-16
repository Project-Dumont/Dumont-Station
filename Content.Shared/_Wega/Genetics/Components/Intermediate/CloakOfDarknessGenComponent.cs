using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class CloakOfDarknessGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string CloakOfDarknessAction = "ActionGenCloakOfDarkness";

    public EntityUid? CloakOfDarknessActionEntity { get; set; }
}
