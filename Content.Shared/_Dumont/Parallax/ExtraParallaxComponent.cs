using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.Parallax;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ExtraParallaxComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string> Parallaxes = new();
}
