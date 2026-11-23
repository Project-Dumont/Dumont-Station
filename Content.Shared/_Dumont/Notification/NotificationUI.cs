using Robust.Shared.Serialization;

namespace Content.Shared.Notification;

[Serializable, NetSerializable]
public enum ModifyNotificationSettingsKey : byte
{
    Key,
}


[Serializable, NetSerializable]
public sealed partial class PdaModifyNotificationSettingsMessage : BoundUserInterfaceMessage
{
    public NotificationSetting Setting;

    public PdaModifyNotificationSettingsMessage(NotificationSetting setting)
    {
        Setting = setting;
    }
}

[Serializable, NetSerializable]
public sealed partial class PdaNotificationSettingsState : BoundUserInterfaceState
{
    public NotificationSetting Setting;

    public PdaNotificationSettingsState(NotificationSetting setting)
    {
        Setting = setting;
    }
}

[Serializable, NetSerializable]
public enum SiliconNotificationsUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class SiliconNotificationBuiState : BoundUserInterfaceState
{
    public List<NotificationMessage> Notifications;

    public SiliconNotificationBuiState(List<NotificationMessage> notifications)
    {
        Notifications = notifications;
    }
}