using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class CryokinesisGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public string ActionId = "ActionCryokinesis";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public float TemperatureDelta = 200f;
}
