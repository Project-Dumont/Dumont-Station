using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class JumpGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string ActionId = "ActionGravityJump";

    [DataField]
    public EntityUid? ActionEntity;
}
