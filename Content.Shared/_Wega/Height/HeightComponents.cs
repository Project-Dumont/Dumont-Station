using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Height;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmallHeightComponent : Component
{
    [DataField]
    public Vector2? OriginalScale;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class BigHeightComponent : Component
{
    [DataField]
    public Vector2? OriginalScale;
}
