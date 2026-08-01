// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Monitor.Components;
using Content.Server.Atmos.Monitor.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Station.Systems;
using Content.Shared._Dumont.Silicons.StationAi;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Doors;
using Content.Shared.Doors.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Silicons.StationAi;

/// <summary>
/// dá pra IA um monitor ao vivo do que ela deveria reagir: alarme de atmos, de incêndio e
/// tripulante barrado numa porta. dá pra pular pra qualquer linha
/// o aviso no chat e o monitor fazem coisas diferentes e os dois são necessários, o aviso
/// conta que aconteceu com a janela fechada, o monitor é onde se resolve.
/// </summary>
public sealed class StationAiAlertsSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedDoorSystem _doors = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationAiWaypointSystem _waypoints = default!;

    /// <summary>
    /// quanto tempo uma área precisa ficar quieta pra avisar a IA no chat de novo
    /// não afeta o monitor, que sempre mostra a situação atual.
    /// </summary>
    private static readonly TimeSpan ChatCooldown = TimeSpan.FromSeconds(60);

    /// <summary>
    /// esbarrar em porta repete rápido então a mesma pessoa na mesma porta só vale
    /// mencionar de vez em quando
    /// </summary>
    private static readonly TimeSpan KnockCooldown = TimeSpan.FromSeconds(20);

    /// <summary>
    /// pedido de porta vence rápido.. ou alguém abriu ou a pessoa foi embora
    /// </summary>
    private static readonly TimeSpan KnockLifetime = TimeSpan.FromSeconds(45);

    private static readonly Color DangerColor = Color.FromHex("#ff4b4b");
    private static readonly Color WarningColor = Color.FromHex("#ffa500");
    private static readonly Color DoorColor = Color.FromHex("#5ed7aa");

    private readonly Dictionary<EntityUid, ActiveAlert> _active = new();
    private readonly Dictionary<(EntityUid Door, EntityUid User), Knock> _knocks = new();
    private readonly Dictionary<(EntityUid? Station, string Area, AtmosAlarmType Type), TimeSpan> _lastChat = new();

    private readonly record struct ActiveAlert(string Area, AiAlertKind Kind, AiAlertSeverity Severity, EntityUid? Station);

    private readonly record struct Knock(string Area, string Who, EntityUid? Station, TimeSpan At);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosAlarmableComponent, AtmosAlarmEvent>(OnAtmosAlarm);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        SubscribeLocalEvent<StationAiWhitelistComponent, BeforeDoorOpenedEvent>(OnDoorAttempt);

        SubscribeLocalEvent<StationAiHeldComponent, ToggleAiAlertsScreenEvent>(OnToggleScreen);
        SubscribeLocalEvent<StationAiHeldComponent, AiAlertWarpMessage>(OnWarp);
        SubscribeLocalEvent<StationAiHeldComponent, AiAlertsRefreshMessage>(OnRefresh);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _active.Clear();
        _knocks.Clear();
        _lastChat.Clear();
    }

    private void OnDoorAttempt(Entity<StationAiWhitelistComponent> ent, ref BeforeDoorOpenedEvent args)
    {
        if (args.User is not { } user)
            return;

        if (_doors.HasAccess(ent.Owner, user))
            return;

        if (HasComp<StationAiHeldComponent>(user) || HasComp<StationAiWhitelistComponent>(user))
            return;

        var now = _timing.CurTime;
        var key = (ent.Owner, user);
        if (_knocks.TryGetValue(key, out var previous) && now < previous.At + KnockCooldown)
            return;

        var station = _station.GetOwningStation(ent.Owner);
        var area = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((ent.Owner, null)));
        var who = Name(user);

        _knocks[key] = new Knock(area, who, station, now);

        var recipients = GetAiFilter(station);
        if (recipients.Count > 0)
        {
            _chat.DispatchFilteredAnnouncement(
                recipients,
                Loc.GetString("station-ai-door-knock", ("who", who), ("area", area)),
                source: ent.Owner,
                sender: Loc.GetString("station-ai-alarm-sender"),
                playSound: false,
                colorOverride: DoorColor);
        }

        RefreshOpenMonitors();
    }

    private void OnAtmosAlarm(EntityUid uid, AtmosAlarmableComponent component, AtmosAlarmEvent args)
    {
        var isFire = HasComp<FireAlarmComponent>(uid);
        if (!isFire && !HasComp<AirAlarmComponent>(uid))
            return;

        if (args.AlarmType is AtmosAlarmType.Warning or AtmosAlarmType.Danger)
        {
            var station = _station.GetOwningStation(uid);

            var area = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((uid, null)));

            var severity = args.AlarmType == AtmosAlarmType.Danger
                ? AiAlertSeverity.Danger
                : AiAlertSeverity.Warning;

            _active[uid] = new ActiveAlert(area, isFire ? AiAlertKind.Fire : AiAlertKind.Atmos, severity, station);
            PingChat(uid, station, area, isFire, args.AlarmType);
        }
        else
        {
            _active.Remove(uid);
        }

        RefreshOpenMonitors();
    }

    private void PingChat(EntityUid source, EntityUid? station, string area, bool isFire, AtmosAlarmType type)
    {
        var now = _timing.CurTime;
        var key = (station, area, type);
        if (_lastChat.TryGetValue(key, out var last) && now < last + ChatCooldown)
            return;

        _lastChat[key] = now;

        var recipients = GetAiFilter(station);
        if (recipients.Count == 0)
            return;

        var message = Loc.GetString(
            type == AtmosAlarmType.Danger ? "station-ai-alarm-danger" : "station-ai-alarm-warning",
            ("kind", Loc.GetString(isFire ? "station-ai-alarm-kind-fire" : "station-ai-alarm-kind-atmos")),
            ("area", area));

        _chat.DispatchFilteredAnnouncement(
            recipients,
            message,
            source: source,
            sender: Loc.GetString("station-ai-alarm-sender"),
            playSound: false,
            colorOverride: type == AtmosAlarmType.Danger ? DangerColor : WarningColor);
    }

    private void OnToggleScreen(Entity<StationAiHeldComponent> ent, ref ToggleAiAlertsScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        args.Handled = true;

        _ui.TryToggleUi(ent.Owner, AiAlertsUiKey.Key, actor.PlayerSession);
        PushState(ent.Owner);
    }

    private void OnRefresh(Entity<StationAiHeldComponent> ent, ref AiAlertsRefreshMessage args)
    {
        PushState(ent.Owner);
    }

    private void OnWarp(Entity<StationAiHeldComponent> ent, ref AiAlertWarpMessage args)
    {
        var target = GetEntity(args.Target);
        if (Deleted(target))
            return;

        if (!_stationAi.TryGetCore(ent.Owner, out var core) || core.Comp?.RemoteEntity == null)
            return;

        if (_xform.GetGrid(core.Owner) != _xform.GetGrid(target))
            return;

        _xform.SetWorldPosition(core.Comp.RemoteEntity.Value, _xform.GetWorldPosition(target));
    }

    private void PushState(EntityUid ai)
    {
        _ui.SetUiState(ai, AiAlertsUiKey.Key, new AiAlertsBuiState(BuildAlerts(ai)));
    }

    private void RefreshOpenMonitors()
    {
        var query = EntityQueryEnumerator<StationAiHeldComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_ui.IsUiOpen(uid, AiAlertsUiKey.Key))
                PushState(uid);
        }
    }

    /// <summary>
    /// linhas vivas dessa IA, descartando o que foi deletado ou venceu.
    /// </summary>
    private List<AiAlertEntry> BuildAlerts(EntityUid ai)
    {
        var station = _station.GetOwningStation(ai);
        var alerts = new List<AiAlertEntry>();

        AddAlarms(station, alerts);
        AddKnocks(station, alerts);
        AddWaypoint(ai, alerts);

        alerts.Sort((a, b) => a.Severity != b.Severity
            ? b.Severity.CompareTo(a.Severity)
            : string.Compare(a.Area, b.Area, StringComparison.CurrentCulture));

        return alerts;
    }

    private void AddAlarms(EntityUid? station, List<AiAlertEntry> alerts)
    {
        var stale = new List<EntityUid>();

        var byArea = new Dictionary<(string Area, AiAlertKind Kind), int>();

        foreach (var (uid, alert) in _active)
        {
            if (Deleted(uid))
            {
                stale.Add(uid);
                continue;
            }

            if (station != null && alert.Station != null && alert.Station != station)
                continue;

            var key = (alert.Area, alert.Kind);
            if (byArea.TryGetValue(key, out var index))
            {
                if (alert.Severity > alerts[index].Severity)
                {
                    var existing = alerts[index];
                    existing.Severity = alert.Severity;
                    existing.Source = GetNetEntity(uid);
                    alerts[index] = existing;
                }

                continue;
            }

            byArea[key] = alerts.Count;
            alerts.Add(new AiAlertEntry
            {
                Source = GetNetEntity(uid),
                Area = alert.Area,
                Kind = alert.Kind,
                Severity = alert.Severity,
            });
        }

        foreach (var uid in stale)
        {
            _active.Remove(uid);
        }
    }

    private void AddKnocks(EntityUid? station, List<AiAlertEntry> alerts)
    {
        var now = _timing.CurTime;
        var stale = new List<(EntityUid, EntityUid)>();

        foreach (var (key, knock) in _knocks)
        {
            if (now > knock.At + KnockLifetime || Deleted(key.Door))
            {
                stale.Add(key);
                continue;
            }

            if (station != null && knock.Station != null && knock.Station != station)
                continue;

            alerts.Add(new AiAlertEntry
            {
                Source = GetNetEntity(key.Door),
                Area = knock.Area,
                Subject = knock.Who,
                Kind = AiAlertKind.Door,
                Severity = AiAlertSeverity.Info,
            });
        }

        foreach (var key in stale)
        {
            _knocks.Remove(key);
        }
    }

    /// <summary>
    /// o ponto da própria IA pra ela voltar pro que marcou pros borgs
    /// </summary>
    private void AddWaypoint(EntityUid ai, List<AiAlertEntry> alerts)
    {
        if (!_waypoints.TryGetWaypoint(ai, out var marker, out var area))
            return;

        alerts.Add(new AiAlertEntry
        {
            Source = GetNetEntity(marker),
            Area = area,
            Kind = AiAlertKind.Waypoint,
            Severity = AiAlertSeverity.Info,
        });
    }

    /// <summary>
    /// toda IA que está vigiando <paramref name="station"/> agora.
    /// </summary>
    private Filter GetAiFilter(EntityUid? station)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<StationAiHeldComponent, ActorComponent>();

        while (query.MoveNext(out var uid, out _, out var actor))
        {
            var aiStation = _station.GetOwningStation(uid);
            if (station != null && aiStation != null && aiStation != station)
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        return filter;
    }
}
