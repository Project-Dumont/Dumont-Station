using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal.Modules;

/// <summary>
/// Base class for bomb module state sent to the client.
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
/// State for the "Maze" module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class MazeModuleState : BombDefusalModuleState
{
    public byte[] WallFlags = new byte[36]; // North=1, South=2, West=4, East=8
    public int PlayerX;
    public int PlayerY;
    public int GoalX;
    public int GoalY;

    public MazeModuleState()
    {
        Type = BombModuleType.Maze;
    }
}

/// <summary>
/// State for the "Memory" module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class MemoryModuleState : BombDefusalModuleState
{
    public int CurrentStage;
    public int DisplayNumber;
    public List<int> ButtonLabels = new();

    public MemoryModuleState()
    {
        Type = BombModuleType.Memory;
    }
}

/// <summary>
/// State for the "Password" module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PasswordModuleState : BombDefusalModuleState
{
    public List<List<char>> Columns = new();
    public List<int> SelectedIndices = new();

    public PasswordModuleState()
    {
        Type = BombModuleType.Password;
    }
}

/// <summary>
/// State for the "Morse Code" module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class MorseCodeModuleState : BombDefusalModuleState
{
    public string MorseSequence = string.Empty;
    public float CurrentFrequency;

    public MorseCodeModuleState()
    {
        Type = BombModuleType.MorseCode;
    }
}

/// <summary>
/// State for the "Who's on First" module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class WhosOnFirstModuleState : BombDefusalModuleState
{
    public string DisplayWord = string.Empty;
    public List<string> ButtonLabels = new();
    public int CurrentStage;

    public WhosOnFirstModuleState()
    {
        Type = BombModuleType.WhosOnFirst;
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
