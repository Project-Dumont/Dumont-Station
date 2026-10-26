using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Robust.Shared.GameStates;
namespace Content.Shared._CMU14.Xenomorphs;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphDespoilerShotVisualComponent : Component
{
    [DataField, AutoNetworkedField] public Vector2 Scale = Vector2.One;
}
