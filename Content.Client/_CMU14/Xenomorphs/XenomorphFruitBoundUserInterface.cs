using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._CMU14.Xenomorphs;

[UsedImplicitly]
public sealed class XenomorphFruitBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private DefaultWindow? _window;
    private Label? _count;

    protected override void Open()
    {
        base.Open();
        _window = new DefaultWindow { Title = Loc.GetString("xenomorph-fruit-choose-title"), MinWidth = 350 };
        var list = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, SeparationOverride = 5 };
        _count = new Label();
        list.AddChild(_count);
        if (EntMan.TryGetComponent<XenomorphFruitPlanterComponent>(Owner, out var planter))
            foreach (var kind in planter.Allowed)
            {
                var name = kind.ToString().ToLowerInvariant();
                var button = new Button { Text = Loc.GetString("xenomorph-fruit-choice-" + name),
                    ToolTip = Loc.GetString("xenomorph-fruit-effect-" + name) };
                button.OnPressed += _ => { SendMessage(new XenomorphChooseFruitMessage(kind)); _window.Close(); };
                list.AddChild(button);
            }
        _window.AddChild(list);
        _window.OnClose += Close;
        _window.OpenCentered();
        if (State != null) UpdateState(State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is XenomorphFruitUiState fruits && _count != null)
            _count.Text = Loc.GetString("xenomorph-fruit-count", ("count", fruits.Count), ("max", fruits.Maximum));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _window?.Dispose();
    }
}
