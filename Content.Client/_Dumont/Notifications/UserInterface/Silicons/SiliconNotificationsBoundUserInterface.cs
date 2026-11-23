using Content.Shared.Notification;
using Content.Shared.Silicons.Borgs;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Serialization;

namespace Content.Client._Dumont.Notifications.UserInterface.Silicons;
[UsedImplicitly]
public sealed class SiliconNotificationsBoundUserInterface : BoundUserInterface
{
    SiliconNotificationsMenu? _menu; 

    public SiliconNotificationsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) 
    {
        
    }
    protected override void Open()
    {
        base.Open();

        CreateMenu();
    }

    private void CreateMenu()
    {
        _menu = this.CreateWindow<SiliconNotificationsMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SiliconNotificationBuiState notificationState)
            return;

        if (_menu is null)
            return;

        if (_menu.Disposed)
            return;

        _menu.UpdateState(notificationState.Notifications);
    }
}