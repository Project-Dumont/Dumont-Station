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
/// Player pressed (mouse down) the button in the Button module.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class PressButtonAction : BombModuleAction
{
}

/// <summary>
/// Player released (mouse up) the button in the Button module.
/// The server checks timing to determine if the release was correct.
/// </summary>
[NetSerializable, Serializable]
public sealed partial class ReleaseButtonAction : BombModuleAction
{
    /// <summary>
    /// The timer digit visible when the player released the button.
    /// Sent by client since client has the timer display.
    /// </summary>
    public int TimerDigit;

    public ReleaseButtonAction(int timerDigit)
    {
        TimerDigit = timerDigit;
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
