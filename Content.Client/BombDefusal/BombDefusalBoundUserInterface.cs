using Content.Shared.BombDefusal;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.BombDefusal;

[UsedImplicitly]
public sealed class BombDefusalBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BombDefusalMenu? _menu;

    public BombDefusalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<BombDefusalMenu>();
        _menu.OnModuleInteraction += (moduleIndex, action) =>
        {
            SendMessage(new BombModuleInteractionMessage(moduleIndex, action));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null)
            return;

        if (state is BombDefusalUiState defusalState)
        {
            _menu.UpdateState(defusalState);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
