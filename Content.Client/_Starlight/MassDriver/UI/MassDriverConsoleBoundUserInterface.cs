using Content.Shared._Starlight.MassDriver;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Starlight.MassDriver.UI;

[UsedImplicitly]
public sealed class MassDriverConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MassDriverConsoleMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MassDriverConsoleMenu>();

        _menu.OnLaunchButtonPressed += () => SendMessage(new MassDriverLaunchMessage());
        _menu.OnModeButtonPressed += mode => SendMessage(new MassDriverModeMessage(mode));
        _menu.OnThrowDistance += distance => SendMessage(new MassDriverThrowDistanceMessage(distance));
        _menu.OnThrowSpeed += speed => SendMessage(new MassDriverThrowSpeedMessage(speed));
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is MassDriverUpdateUIMessage massDriverMessage)
            _menu?.UpdateState(massDriverMessage.State);
    }
}
