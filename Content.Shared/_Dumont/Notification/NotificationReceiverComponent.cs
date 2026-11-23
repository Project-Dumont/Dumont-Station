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

[Serializable, NetSerializable]
public record NotificationMessage(TimeSpan Time, string Message);

public enum NotificationSetting
{
    Loud,
    Normal,
    Silent
}