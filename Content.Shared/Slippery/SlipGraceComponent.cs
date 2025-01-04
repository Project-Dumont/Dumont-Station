using Robust.Shared.GameStates;

namespace Content.Shared.Slippery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlipGraceComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan Delay;
}
