using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal.Modules;

/// <summary>
/// This is the list of all available bomb modules that can be rolled for the bomb when armed. Add to this if you want to add more modules.
/// </summary>
[NetSerializable, Serializable]
public enum BombModuleType : byte
{
    Wires,
    Symbols,
    SimonSays,
    Codewords,
    Maze,
    Memory,
    Password,
    MorseCode,
    WhosOnFirst,
}
