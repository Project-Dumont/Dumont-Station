using Robust.Shared.GameStates;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Marker component for the Bomb Defusal Kit item.
/// The kit must be wielded (two hands) to interact with a bomb.
/// Dropping or unwielding the kit while defusing closes the UI and triggers a strike.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BombDefusalKitComponent : Component
{
    /// <summary>
    /// The bomb entity this kit is currently linked to (has UI open for), if any.
    /// </summary>
    [AutoNetworkedField]
    public EntityUid? LinkedBomb;
}
