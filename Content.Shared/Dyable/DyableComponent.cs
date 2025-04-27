using Robust.Shared.GameStates;

namespace Content.Shared.Dyable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class DyableComponent : Component
{
    [DataField, AutoNetworkedField, Access(typeof(SharedDyableSystem))]
    public Color Color = Color.White;
}
