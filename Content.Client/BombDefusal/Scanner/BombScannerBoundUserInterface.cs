using Content.Shared.BombDefusal;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.BombDefusal.Scanner;

[UsedImplicitly]
public sealed class BombScannerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BombScannerMenu? _menu;

    public BombScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<BombScannerMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not BombScannerUiState scannerState || _menu == null)
            return;

        _menu.Populate(scannerState.RuleSet);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
