using Robust.Shared.Serialization;

namespace Content.Shared.Notification;

[RegisterComponent]
public sealed partial class NotificationReceiverComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<NotificationMessage> Notifications = [];

    [ViewVariables(VVAccess.ReadWrite)]
    public NotificationSetting Setting = NotificationSetting.Normal;

}

public sealed class NotificationReceiverState : IComponentState
{
    public List<NotificationMessage> Notifications = [];

    public NotificationReceiverState(List<NotificationMessage> notifications)
    {
        Notifications = notifications;
    }
}

[Serializable, NetSerializable]
public record NotificationMessage(TimeSpan Time, string Message);

public enum NotificationSetting
{
    Loud,
    Normal,
    Silent
}