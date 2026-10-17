using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// Abstract base class for server-side bomb module logic.
/// Each module type implements generation, state serialization, and answer validation.
/// </summary>
public abstract class BombModule
{
    public BombModuleType Type;
    public bool IsSolved;

    /// <summary>
    /// Get the visible state to send to the client UI.
    /// </summary>
    public abstract BombDefusalModuleState GetVisibleState();

    /// <summary>
    /// Validate a player's action on this module.
    /// Returns true if the action was correct (or partially correct for multi-step modules).
    /// Returns false if the action was wrong (should trigger a strike).
    /// </summary>
    public abstract bool ValidateAction(BombModuleAction action);
}
