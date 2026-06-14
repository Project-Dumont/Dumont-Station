using Content.Shared.BombDefusal;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.BombDefusal.Manual;

[UsedImplicitly]
public sealed class BombDefusalManualBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BombDefusalManualMenu? _menu;

    public BombDefusalManualBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<BombDefusalManualMenu>();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
