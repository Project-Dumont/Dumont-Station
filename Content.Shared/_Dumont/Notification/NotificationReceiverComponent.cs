using Robust.Shared.Serialization;

namespace Content.Shared.Notification;

[RegisterComponent]
public sealed partial class NotificationReceiverComponent : Component
{
    [ViewVariables] 
    public List<NotificationMessage> Notifications = [];

}

[Serializable, NetSerializable]
public record NotificationMessage(TimeSpan Time, string Message);