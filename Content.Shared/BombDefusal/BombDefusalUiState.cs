using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Full UI state sent from server to client for the bomb defusal interface.
/// </summary>
[NetSerializable, Serializable]
public sealed class BombDefusalUiState : BoundUserInterfaceState
{
    /// <summary>
    /// State for each module on the bomb.
    /// </summary>
    public List<BombDefusalModuleState> Modules = new();

    /// <summary>
    /// Current number of strikes received.
    /// </summary>
    public int Strikes;

    /// <summary>
    /// Maximum number of strikes before detonation.
    /// </summary>
    public int MaxStrikes;

    /// <summary>
    /// The bomb's random serial number (e.g., "AB3D4F"), used by manual lookup rules.
    /// </summary>
    public string SerialNumber = string.Empty;

    /// <summary>
    /// Remaining time on the timer in seconds.
    /// </summary>
    public float RemainingTime;

    /// <summary>
    /// Whether the bomb is currently armed.
    /// </summary>
    public bool IsActive;
}
