using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class CryokinesisGenComponent : Component
{
    public EntProtoId ActionId = "ActionCryokinesis";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public float TemperatureDelta = 200f;
}
