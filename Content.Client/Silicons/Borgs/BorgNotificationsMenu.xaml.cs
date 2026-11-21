using Content.Client.UserInterface.Controls;
using Content.Shared.Notification;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

class BorgNotificationsMenu : FancyWindow
{
    List<NotificationMessage> notifications = new List<NotificationMessage>();
    public BorgNotificationsMenu() 
    {
        RobustXamlLoader.Load(this);

        UpdateState(notifications);    
    }

    public void UpdateState(List<NotificationMessage> notifs)
    {
        notifications = notifs;
        NotificationsContainer.RemoveAllChildren();

        foreach (NotificationMessage notis in notifications)
        {
            BoxContainer notifContainer = new BoxContainer() { Orientation = BoxContainer.LayoutOrientation.Horizontal, Margin = new Thickness(3, 7) };
            RichTextLabel time = new RichTextLabel() { Text = notis.Time.ToString(@"hh\:mm"), Margin = new Thickness(15, 0) };
            RichTextLabel message = new RichTextLabel() { Text = notis.Message };

            notifContainer.AddChild(time);
            notifContainer.AddChild(message);
            NotificationsContainer.AddChild(notifContainer);
        }
    }
}