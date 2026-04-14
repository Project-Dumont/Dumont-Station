using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class CloakOfDarknessGenComponent : Component
{
    public readonly EntProtoId CloakOfDarknessAction = "ActionGenCloakOfDarkness";

    public EntityUid? CloakOfDarknessActionEntity { get; set; }
}
