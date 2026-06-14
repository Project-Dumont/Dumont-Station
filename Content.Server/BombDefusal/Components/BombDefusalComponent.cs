using Content.Server.BombDefusal.Modules;
using Content.Server.BombDefusal.Systems;
using Robust.Shared.Audio;

namespace Content.Server.BombDefusal.Components;

/// <summary>
/// Component for KTANE-style bomb defusal. Replaces wire-based defusal.
/// Module count is derived from the bomb's timer duration (1 per minute).
/// </summary>
[RegisterComponent, Access(typeof(BombDefusalSystem))]
public sealed partial class BombDefusalComponent : Component
{
    /// <summary>
    /// Override for module count. If null, derived from timer (seconds / 60).
    /// </summary>
    [DataField]
    public int? ModuleCountOverride;

    /// <summary>
    /// Maximum strikes before detonation.
    /// </summary>
    [DataField]
    public int MaxStrikes = 3;

    /// <summary>
    /// Current strike count.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Strikes;

    /// <summary>
    /// The bomb's serial number, randomly generated on init.
    /// Used by module rules (e.g., last digit odd/even, vowel presence).
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public string SerialNumber = string.Empty;

    /// <summary>
    /// The generated bomb modules.
    /// </summary>
    [ViewVariables]
    public List<BombModule> Modules = new();

    /// <summary>
    /// Whether the bomb has been fully defused (all modules solved).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsDefused;

    /// <summary>
    /// Whether the modules have been generated yet.
    /// </summary>
    [ViewVariables]
    public bool ModulesGenerated;

    /// <summary>
    /// Sound played when a strike is added.
    /// </summary>
    [DataField]
    public SoundSpecifier StrikeSound = new SoundPathSpecifier("/Audio/Machines/buzz-sigh.ogg");

    /// <summary>
    /// Sound played when a module is solved.
    /// </summary>
    [DataField]
    public SoundSpecifier SolveSound = new SoundPathSpecifier("/Audio/Machines/ping.ogg");

    /// <summary>
    /// Sound played when the entire bomb is defused.
    /// </summary>
    [DataField]
    public SoundSpecifier DefuseSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");
}
