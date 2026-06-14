using Robust.Shared.GameStates;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Marker component for the Bomb Defusal Manual item.
/// The manual contains static defusal rules and is opened via ActivatableUI.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BombDefusalManualComponent : Component
{
}
