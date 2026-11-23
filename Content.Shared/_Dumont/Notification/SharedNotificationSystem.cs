using Content.Shared.Notification;

public sealed class SharedNotificationSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotificationReceiverComponent, PdaModifyNotificationSettingsMessage>(OnModifySettings);

    }
    public void OpenNotificationSettingsUI(EntityUid uid, EntityUid actor, NotificationReceiverComponent comp)
    {
        if (!TryComp<UserInterfaceComponent>(uid, out _))
            return;
        
        UpdateUi((uid, comp));
        _ui.TryToggleUi(uid, ModifyNotificationSettingsKey.Key, actor);
    }

    private void OnModifySettings(EntityUid receiverUid, NotificationReceiverComponent comp, PdaModifyNotificationSettingsMessage msg)
    {
        UpdateUi((receiverUid, comp), msg.Setting);
    }

    private void UpdateUi(Entity<NotificationReceiverComponent> receiver, NotificationSetting? setting = null)
    {
        if (!TryComp<UserInterfaceComponent>(receiver.Owner, out var userInterfaceComponent))
            return;

        if (setting is { } notiSetting)
            receiver.Comp.Setting = notiSetting;

        var state = new PdaNotificationSettingsState(receiver.Comp.Setting);
        _ui.SetUiState((receiver.Owner, userInterfaceComponent), ModifyNotificationSettingsKey.Key, state);
    }
}