using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class JumpGenComponent : Component
{
    public readonly EntProtoId ActionId = "ActionJumpGen";

    [DataField]
    public EntityUid? ActionEntity;
}
