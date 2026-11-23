using Content.Shared.Notification;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Dumont.Notifications.UserInterface;

[UsedImplicitly]
public sealed partial class NotificationSettingsBUI : BoundUserInterface
{
    NotificationSettingsWindow? _window;

    int? _selectedButton;
    public NotificationSettingsBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey) 
    {
        
    }

    protected override void Open()
    {
        base.Open();

        if (_window is not null)
            return;

        _window = this.CreateWindow<NotificationSettingsWindow>();
        var child  = _window.GetChild(_selectedButton ?? 0);
        if (child is not Button setting)
            return;

        setting.Pressed = true;

        _window.LoudNotificationSetting.OnPressed += _ => { SendMessage(new PdaModifyNotificationSettingsMessage(NotificationSetting.Loud)); };
        _window.NormalNotificationSetting.OnPressed += _ => { SendMessage(new PdaModifyNotificationSettingsMessage(NotificationSetting.Normal)); };
        _window.SilentNotificationSetting.OnPressed += _ => { SendMessage(new PdaModifyNotificationSettingsMessage(NotificationSetting.Silent)); };

    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window is null)
            return;

        if (state is not PdaNotificationSettingsState settingsState)
            return;


        _selectedButton = (int)settingsState.Setting;
    }
}