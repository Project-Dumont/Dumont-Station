using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

/// <summary>
/// Base class for module-specific interaction payloads sent from client to server.
/// </summary>
[NetSerializable, Serializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class BombModuleAction
{
}

/// <summary>
/// Player cut a wire in the Wires module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class CutWireAction : BombModuleAction
{
    public int WireIndex;

    public CutWireAction(int wireIndex)
    {
        WireIndex = wireIndex;
    }
}

/// <summary>
/// Player pressed a symbol button in the Symbols module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressSymbolAction : BombModuleAction
{
    public int SymbolIndex;

    public PressSymbolAction(int symbolIndex)
    {
        SymbolIndex = symbolIndex;
    }
}

/// <summary>
/// Player moved in the Maze module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressMazeDirectionAction : BombModuleAction
{
    public int Dx;
    public int Dy;

    public PressMazeDirectionAction(int dx, int dy)
    {
        Dx = dx;
        Dy = dy;
    }
}

/// <summary>
/// Player pressed a button in the Memory module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressMemoryButtonAction : BombModuleAction
{
    public int ButtonIndex;

    public PressMemoryButtonAction(int buttonIndex)
    {
        ButtonIndex = buttonIndex;
    }
}

/// <summary>
/// Player cycled a letter column in the Password module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class CyclePasswordColumnAction : BombModuleAction
{
    public int ColumnIndex;
    public bool Up;

    public CyclePasswordColumnAction(int columnIndex, bool up)
    {
        ColumnIndex = columnIndex;
        Up = up;
    }
}

/// <summary>
/// Player submitted the password in the Password module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class SubmitPasswordAction : BombModuleAction
{
}

/// <summary>
/// Player cycled the frequency in the Morse Code module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class CycleMorseFrequencyAction : BombModuleAction
{
    public bool Up;

    public CycleMorseFrequencyAction(bool up)
    {
        Up = up;
    }
}

/// <summary>
/// Player submitted the frequency in the Morse Code module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class SubmitMorseAction : BombModuleAction
{
}

/// <summary>
/// Player pressed a button in the Who's on First module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressWhosOnFirstButtonAction : BombModuleAction
{
    public int ButtonIndex;

    public PressWhosOnFirstButtonAction(int buttonIndex)
    {
        ButtonIndex = buttonIndex;
    }
}

/// <summary>
/// Player pressed a colored button in the Simon Says module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressSimonColorAction : BombModuleAction
{
    public SimonColor Color;

    public PressSimonColorAction(SimonColor color)
    {
        Color = color;
    }
}

/// <summary>
/// Player selected a word in the Codewords module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class SubmitCodewordAction : BombModuleAction
{
    public int WordIndex;

    public SubmitCodewordAction(int wordIndex)
    {
        WordIndex = wordIndex;
    }
}

/// <summary>
/// Message from client to server: player interacted with a specific module.
/// </summary>
[NetSerializable, Serializable]
public sealed class BombModuleInteractionMessage : BoundUserInterfaceMessage
{
    /// <summary>
    /// Index of the module in the bomb's module list.
    /// </summary>
    public int ModuleIndex;

    /// <summary>
    /// The specific action the player performed.
    /// </summary>
    public BombModuleAction Action;

    public BombModuleInteractionMessage(int moduleIndex, BombModuleAction action)
    {
        ModuleIndex = moduleIndex;
        Action = action;
    }
}
