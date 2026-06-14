using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// Base class for module UI controls in the bomb defusal interface.
/// </summary>
public abstract class BaseModuleControl : BoxContainer
{
    public int ModuleIndex;
    public event Action<BombModuleAction>? OnAction;

    protected void RaiseAction(BombModuleAction action)
    {
        OnAction?.Invoke(action);
    }

    public abstract void UpdateState(BombDefusalModuleState state);

    protected BaseModuleControl()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        VerticalExpand = true;
        Margin = new Thickness(4);
    }
}
