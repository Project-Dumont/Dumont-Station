using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal.Modules;

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
