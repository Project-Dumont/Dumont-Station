using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class FarVisionGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public string ActionId = "ActionFarVisionGen";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public float MaxOffset = 10f;

    [DataField]
    public float OffsetSpeed = 10f;

    [DataField]
    public float PvsIncrease = 1.0f;
}
