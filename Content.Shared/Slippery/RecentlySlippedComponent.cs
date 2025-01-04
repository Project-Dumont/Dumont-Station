using Robust.Shared.GameStates;

namespace Content.Shared.Slippery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecentlySlipppedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextSlip;
}
