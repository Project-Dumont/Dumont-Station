using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal.Modules;

/// <summary>
/// Base class for bomb module visible state sent to the client.
/// Each module type derives from this with its own display data.
/// </summary>
[NetSerializable, Serializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class BombDefusalModuleState
{
    public BombModuleType Type;
    public bool IsSolved;
}

/// <summary>
/// Wire colors used by the Wires module.
/// </summary>
[NetSerializable, Serializable]
public enum WireColor : byte
{
    Red,
    Blue,
    Yellow,
    White,
    Black,
}

/// <summary>
/// State for the "Simple Wires" module.
/// 3-6 colored wires; the defuser must cut the correct one.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class WiresModuleState : BombDefusalModuleState
{
    /// <summary>
    /// The colors of each wire, in order from top to bottom.
    /// </summary>
    public List<WireColor> WireColors = new();

    /// <summary>
    /// Which wires have been cut (by index). Used to render severed wires.
    /// </summary>
    public HashSet<int> CutWires = new();

    public WiresModuleState()
    {
        Type = BombModuleType.Wires;
    }
}

/// <summary>
/// State for the "Symbols/Keypads" module.
/// 4 symbols on buttons; press in the correct order.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class SymbolsModuleState : BombDefusalModuleState
{
    /// <summary>
    /// The 4 symbol IDs displayed on the buttons.
    /// Symbols are indices into a shared symbol table.
    /// </summary>
    public List<int> SymbolIds = new();

    /// <summary>
    /// Symbols that have been correctly pressed so far, in order.
    /// </summary>
    public List<int> PressedSymbols = new();

    public SymbolsModuleState()
    {
        Type = BombModuleType.Symbols;
    }
}

/// <summary>
/// Colors for the Button module.
/// </summary>
[NetSerializable, Serializable]
public enum ButtonColor : byte
{
    Red,
    Blue,
    Yellow,
    White,
}

/// <summary>
/// Labels for the Button module.
/// </summary>
[NetSerializable, Serializable]
public enum ButtonLabel : byte
{
    Abort,
    Detonate,
    Hold,
    Press,
}

/// <summary>
/// State for "The Button" module.
/// A single colored button with a label; tap or hold based on rules.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class ButtonModuleState : BombDefusalModuleState
{
    public ButtonColor Color;
    public ButtonLabel Label;

    /// <summary>
    /// The color of the strip revealed when holding. Null if not currently held.
    /// </summary>
    public ButtonColor? StripColor;

    /// <summary>
    /// Whether the button is currently being held down.
    /// </summary>
    public bool IsHeld;

    public ButtonModuleState()
    {
        Type = BombModuleType.Button;
    }
}

/// <summary>
/// Colors used in the Simon Says module.
/// </summary>
[NetSerializable, Serializable]
public enum SimonColor : byte
{
    Red,
    Blue,
    Green,
    Yellow,
}

/// <summary>
/// State for the "Simon Says" module.
/// Colored lights flash in a sequence; press remapped colors.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class SimonSaysModuleState : BombDefusalModuleState
{
    /// <summary>
    /// The sequence of colors that flash for the current stage.
    /// Grows by 1 each stage.
    /// </summary>
    public List<SimonColor> FlashSequence = new();

    /// <summary>
    /// How many stages total must be completed.
    /// </summary>
    public int TotalStages;

    /// <summary>
    /// Current stage (0-indexed). Stage N means the sequence has N+1 colors.
    /// </summary>
    public int CurrentStage;

    /// <summary>
    /// How many colors the player has correctly pressed in the current stage.
    /// </summary>
    public int InputProgress;

    /// <summary>
    /// Whether the module is currently playing the flash sequence (client should animate).
    /// </summary>
    public bool IsFlashing;

    public SimonSaysModuleState()
    {
        Type = BombModuleType.SimonSays;
    }
}

/// <summary>
/// State for the "Codewords" module.
/// 6 words displayed; select the correct one.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class CodewordsModuleState : BombDefusalModuleState
{
    /// <summary>
    /// The 6 words displayed to the defuser.
    /// </summary>
    public List<string> Words = new();

    /// <summary>
    /// Index of the word the player has selected, or -1 if none.
    /// </summary>
    public int SelectedIndex = -1;

    public CodewordsModuleState()
    {
        Type = BombModuleType.Codewords;
    }
}
