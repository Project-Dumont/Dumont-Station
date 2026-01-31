using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.ServerCurrency.Ghost;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class GhostSkinComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<GhostSkinListingPrototype>? Skin;
}
