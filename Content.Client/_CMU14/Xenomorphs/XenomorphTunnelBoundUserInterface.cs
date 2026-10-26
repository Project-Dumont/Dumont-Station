using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._CMU14.Xenomorphs;

[UsedImplicitly]
public sealed class XenomorphTunnelBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private XenomorphTunnelWindow? _window;
    private NetEntity? _destination;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<XenomorphTunnelWindow>();
        var renaming = UiKey.Equals(XenomorphTunnelUiKey.Rename);
        _window.NameRow.Visible = renaming;
        _window.Destinations.Visible = !renaming;
        _window.TravelButton.Visible = !renaming;
        _window.ExitButton.Visible = !renaming;
        _window.Destinations.OnItemSelected += args =>
        {
            _destination = args.ItemList[args.ItemIndex].Metadata as NetEntity?;
            _window.TravelButton.Disabled = _destination == null;
        };
        _window.TravelButton.OnPressed += _ =>
        {
            if (_destination is { } destination)
                SendMessage(new XenomorphTunnelTravelMessage(destination));
        };
        _window.RenameButton.OnPressed += _ => SendMessage(new XenomorphTunnelNameMessage(_window.NameEdit.Text));
        _window.ExitButton.OnPressed += _ => { SendMessage(new XenomorphTunnelExitMessage()); Close(); };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not XenomorphTunnelUiState tunnels) return;
        _window.Title = tunnels.Name;
        _window.NameEdit.Text = tunnels.Name;
        _window.Destinations.Clear();
        _destination = null;
        _window.TravelButton.Disabled = true;
        foreach (var (entity, name) in tunnels.Tunnels)
            _window.Destinations.Add(new ItemList.Item(_window.Destinations) { Text = name, Metadata = entity });
    }
}
