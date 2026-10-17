using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Election;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ElectionScreenComponent : Component
{
    [DataField, AutoNetworkedField]
    public ElectionScreenMode Mode = ElectionScreenMode.Scrolling;

    /// <summary>
    /// top left of the lit area, in sprite pixels from the top left of the sprite
    /// </summary>
    [DataField]
    public Vector2 ScreenPosition = new(4f, 8f);

    [DataField]
    public Vector2 ScreenSize = new(88f, 48f);
}

/// <summary>
/// marks where the election screen drop pod lands
/// </summary>
[RegisterComponent]
public sealed partial class ElectionScreenSpawnerComponent : Component
{
    [DataField]
    public EntProtoId Prototype = "SpawnPodElectionScreen";
}

[Serializable, NetSerializable]
public enum ElectionScreenMode : byte
{
    Scrolling,
    Top3,
}
