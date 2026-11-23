using Content.Shared.Cargo.Components;
using Content.Shared.Notification;
using Content.Shared.PDA;
using Content.Shared.Pinpointer;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

public sealed partial class SiliconNotificationSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedStationSystem _station = default!;
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
            if (!HandleSiliconNotification(ev, uid, receiverComponent))
                continue;

            Dirty<NotificationReceiverComponent>((uid, receiverComponent));
            UpdateNotificationUI(uid, receiverComponent);
        }

        var ai = EntityQueryEnumerator<StationAiHeldComponent, NotificationReceiverComponent>();
        while (ai.MoveNext(out var uid, out var comp, out var receiverComponent))
        {
            if (!HandleSiliconNotification(ev, uid, receiverComponent))
                continue;

            Dirty<NotificationReceiverComponent>((uid, receiverComponent));
            UpdateNotificationUI(uid, receiverComponent);
        }

    }

    private bool HandleSiliconNotification(NotificationEvent ev, EntityUid uid, NotificationReceiverComponent receiverComponent)
    {
        if (ev.Station is { } station && _station.GetOwningStation(uid) != station)
            return false;

        receiverComponent.Notifications.Add(new NotificationMessage(_timing.CurTime, ev.Message));
        return true;
        
    }

    public void UpdateNotificationUI(EntityUid uid, NotificationReceiverComponent comp)
    {

        var state = new SiliconNotificationBuiState(comp.Notifications);
        _ui.SetUiState(uid, SiliconNotificationsUiKey.Key, state);
    }
}