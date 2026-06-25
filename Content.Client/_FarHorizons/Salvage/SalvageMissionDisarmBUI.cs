using Content.Shared._FarHorizons.Salvage.Components;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Salvage;

public sealed partial class SalvageMissionDisarmBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SalvageMissionDisarmWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SalvageMissionDisarmWindow>();
        _window.SubmitCodeCallback += SubmitCode;
    }

    private void SubmitCode(int code) =>
        SendMessage(new SalvageMissionDisarmSubmitCodeMessage(code));
}