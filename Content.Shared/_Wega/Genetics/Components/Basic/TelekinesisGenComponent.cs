using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class TelekinesisGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>, DataField("itemPrototype"), ViewVariables(VVAccess.ReadWrite)]
    public string ItemPrototype = "HandTelekinesisGun";

    [DataField("handId"), ViewVariables(VVAccess.ReadWrite)]
    public string HandId = "telekinesis-hand";

    [ViewVariables]
    public EntityUid? TelekinesisItem;
}