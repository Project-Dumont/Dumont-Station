using Robust.Shared.Serialization;

namespace Content.Shared.Notification;

[RegisterComponent]
public sealed partial class NotificationReceiverComponent : Component
{
    [ViewVariables]
    public List<NotificationMessage> Notifications = [];

    [ViewVariables]
    public NotificationSetting Setting = NotificationSetting.Normal;

}

[Serializable, NetSerializable]
public record NotificationMessage(TimeSpan Time, string Message);

public enum NotificationSetting
{
    Loud,
    Normal,
    Silent
}