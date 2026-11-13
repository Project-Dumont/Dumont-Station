using Content.Shared.Atmos.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.RPD;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RpdLayerSelectorSystem))]
public sealed partial class RpdLayerSelectorComponent : Component
{
    [DataField, AutoNetworkedField]
    public AtmosPipeLayer? SelectedLayer;
}
