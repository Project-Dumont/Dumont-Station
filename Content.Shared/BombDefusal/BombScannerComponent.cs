using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Component for the Bomb Scanner to list the randomized rules for the specific bomb.
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
