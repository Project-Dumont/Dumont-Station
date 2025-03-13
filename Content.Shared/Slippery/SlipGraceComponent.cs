using Robust.Shared.GameStates;

namespace Content.Shared.Slippery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlipGraceComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public TimeSpan Delay;

    /// <summary>
    ///     Should super slippery stuff such as lube have a grace period.
    /// </summary>
    [DataField]
    public bool SuperSlippery = false;
}
