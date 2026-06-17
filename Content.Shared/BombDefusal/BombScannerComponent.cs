using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Component for the Bomb Scanner item used to read randomized defusal rules from a bomb.
/// </summary>
[RegisterComponent]
public sealed partial class BombScannerComponent : Component
{
    /// <summary>
    /// The bomb entity that was scanned.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? ScannedBomb;
}
