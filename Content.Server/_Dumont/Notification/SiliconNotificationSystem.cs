using Content.Shared.Cargo.Components;
using Content.Shared.Notification;
using Content.Shared.PDA;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Timing;

public sealed partial class SiliconNotificationSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NotificationEvent>(OnNotification);
    }

    private void OnNotification(NotificationEvent ev)
    {
        var borgs = EntityQueryEnumerator<BorgChassisComponent, NotificationReceiverComponent>();
        while (borgs.MoveNext(out var uid, out var comp, out var receiverComponent))
        {
            receiverComponent.Notifications.Add(new NotificationMessage(_timing.CurTime, ev.Message));
            UpdateNotificationUI(uid, receiverComponent);
        }

        var ai = EntityQueryEnumerator<StationAiHeldComponent, NotificationReceiverComponent>();
        while (ai.MoveNext(out var uid, out var comp, out var receiverComponent))
        {
            receiverComponent.Notifications.Add(new NotificationMessage(_timing.CurTime, ev.Message));
            UpdateNotificationUI(uid, receiverComponent);
        }

    }

    public void UpdateNotificationUI(EntityUid uid, NotificationReceiverComponent comp)
    {

        var state = new SiliconNotificationBuiState(comp.Notifications);
        _ui.SetUiState(uid, SiliconNotificationsUiKey.Key, state);
    }
}