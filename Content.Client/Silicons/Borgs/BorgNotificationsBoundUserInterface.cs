using Content.Shared.Silicons.Borgs;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Silicons.Borgs;
[UsedImplicitly]
public sealed class BorgNotificationsBoundUserInterface : BoundUserInterface
{
    BorgNotificationsMenu? _menu; 

    public BorgNotificationsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) 
    {
        
    }
    protected override void Open()
    {
        base.Open();

        CreateMenu();
    }

    private void CreateMenu()
    {
        _menu = this.CreateWindow<BorgNotificationsMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BorgNotificationBuiState notificationState)
            return;

        if (_menu is null)
            return;

        if (_menu.Disposed)
            return;

        _menu.UpdateState(notificationState.Notifications);
    }
}