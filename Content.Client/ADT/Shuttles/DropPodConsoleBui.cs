// Seu arquivo DropPodConsoleBui.cs simplificado
using Content.Shared.ADT.Shuttles.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.ADT.Shuttles;

[UsedImplicitly]
public sealed class DropPodConsoleBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private DropPodConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = new DropPodConsoleWindow();
        _window.OnDeployMessage += SendDeployMessage;
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState? state)
    {
        if (state is not DropPodConsoleBuiState s)
            return;

        _window?.UpdateState(s);
    }

    private void SendDeployMessage(NetEntity targetBeacon)
    {
        SendMessage(new DropPodConsoleDeployMessage { TargetBeacon = targetBeacon });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Close();
    }
}