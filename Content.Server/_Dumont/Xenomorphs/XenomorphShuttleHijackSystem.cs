using System.Linq;
using Content.Shared.Shuttles.Components;
using Content.Shared._CMU14.Xenomorphs;
using Content.Server._White.GameTicking.Rules;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Xenomorphs;

/// <summary>
/// Alternative victory route after the crew's evacuation docks: take a shuttle to
/// the same Centcomm, before the crew arrives, and keep the round running for combat.
/// </summary>
public sealed partial class XenomorphShuttleHijackSystem : EntitySystem
{
    [Dependency] private XenomorphHiveObjectivesSystem _objectives = default!;
    [Dependency] private XenomorphHiveCaptureSystem _hive = default!;
    [Dependency] private EmergencyShuttleSystem _emergency = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private XenomorphsRuleSystem _rules = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private GameTicker _ticker = default!;

    public bool CommsLocked { get; private set; }
    public bool CrewArrived { get; private set; }
    public bool Invaded { get; private set; }
    public EntityUid? HijackedShuttle { get; private set; }
    public bool WindowClosed => CrewArrived && HijackedShuttle == null && HijackingQueen == null;
    public TimeSpan? FinaleEndsAt { get; private set; }
    private EntityUid? _evac;
    private EntityUid? _centcomm;
    private EntityUid? _centcommMap;
    private TimeSpan? _expectedCrewArrival;
    private bool _finishing;
    private readonly HashSet<EntityUid> _evacuees = new();
    public bool CrewEscaped => _evacuees.Any(uid => Exists(uid) && !_mobs.IsDead(uid) && Transform(uid).MapUid == _centcommMap);

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<EvacuationRequestAttemptEvent>(OnEvacuationRequest);
        SubscribeLocalEvent<RoundEndAttemptEvent>(OnRoundEndAttempt);
        SubscribeLocalEvent<EmergencyShuttleArrivedAtStationEvent>(OnEvacDocked);
        SubscribeLocalEvent<FTLStartedEvent>(OnEvacDeparted);
        SubscribeLocalEvent<FTLCompletedEvent>(OnEvacArrived);
        SubscribeLocalEvent<XenomorphHijackedShuttleComponent, FTLCompletedEvent>(OnInvasionArrived);
        InitializeInteraction();
    }

    private void OnRestart(RoundRestartCleanupEvent args)
    {
        CommsLocked = false;
        CrewArrived = false;
        Invaded = false;
        HijackedShuttle = null;
        FinaleEndsAt = null;
        _evac = null;
        _centcomm = null;
        _centcommMap = null;
        _expectedCrewArrival = null;
        _finishing = false;
        ResetHijackInteraction();
        _evacuees.Clear();
    }

    private bool HasLivingXenos()
    {
        var query = EntityQueryEnumerator<XenomorphComponent>();
        while (query.MoveNext(out var uid, out _))
            if (!_mobs.IsDead(uid)) return true;
        return false;
    }

    private void AnnounceCommunicationError()
        => _chat.DispatchGlobalAnnouncement(Loc.GetString("xenomorph-evac-comms-error"), "S.A.A.E.", colorOverride: Color.Red);

    private void OnEvacuationRequest(ref EvacuationRequestAttemptEvent args)
    {
        if (_objectives.HijackingQueen == null) return;
        args.Cancelled = true;
        AnnounceCommunicationError();
    }

    public void OnCommsHijackStarted()
    {
        if (!_roundEnd.IsRoundEndRequested() || _emergency.EmergencyShuttleArrived) return;
        _roundEnd.CancelRoundEndCountdown(checkCooldown: false, announce: false);
        AnnounceCommunicationError();
    }

    private void OnEvacDocked(ref EmergencyShuttleArrivedAtStationEvent args)
    {
        if (CommsLocked || _objectives.CommsCorrupted || !HasLivingXenos()) return;
        CommsLocked = true;
        _evac = args.Shuttle;
        _emergency.TransitTime = 60f;
        if (_objectives.HijackingQueen is { } queen) _objectives.CancelHijack(queen);
        var stations = EntityQueryEnumerator<StationEmergencyShuttleComponent, StationCentcommComponent>();
        while (stations.MoveNext(out _, out var shuttle, out var centcomm))
        {
            if (shuttle.EmergencyShuttle != args.Shuttle) continue;
            _centcomm = centcomm.Entity;
            _centcommMap = centcomm.MapEntity;
            break;
        }
        _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-rally"));
        _objectives.RefreshHiveObjectives();
    }

    private void OnEvacDeparted(ref FTLStartedEvent args)
    {
        if (!CommsLocked || args.Entity != _evac) return;
        var passengers = EntityQueryEnumerator<Content.Shared.Mobs.Components.MobStateComponent, TransformComponent>();
        while (passengers.MoveNext(out var uid, out _, out var transform))
            if (transform.GridUid == _evac && !HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid)
                && (HasComp<Content.Shared.Humanoid.HumanoidAppearanceComponent>(uid)
                    || TryComp<Content.Shared.Mind.Components.MindContainerComponent>(uid, out var mind) && mind.HasMind))
                _evacuees.Add(uid);
        _expectedCrewArrival = _timing.CurTime + TimeSpan.FromSeconds(
            TryComp<FTLComponent>(args.Entity, out var ftl) ? ftl.TravelTime : 60);
    }

    private void OnEvacArrived(ref FTLCompletedEvent args)
    {
        if (!CommsLocked || args.Entity != _evac || args.MapUid != _centcommMap) return;
        CompleteCrewArrival();
    }

    private void CompleteCrewArrival()
    {
        if (CrewArrived) return;
        CrewArrived = true;
        if (HijackedShuttle != null || HijackingQueen != null)
            ExtendFinale(_timing.CurTime + TimeSpan.FromMinutes(10));
        else
            _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-window-lost"));
    }

    private void ExtendFinale(TimeSpan until)
    {
        if (FinaleEndsAt == null || until > FinaleEndsAt) FinaleEndsAt = until;
    }

    private void OnRoundEndAttempt(ref RoundEndAttemptEvent args)
    {
        if (_finishing || !CommsLocked || HijackedShuttle == null && HijackingQueen == null) return;
        if (!Invaded && !HasLivingXenos()) return;
        args.Cancelled = !CrewArrived || FinaleEndsAt == null || _timing.CurTime < FinaleEndsAt;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateLandingEffects();
        // Also close the window if the crew shuttle is deleted while finishing FTL.
        if (!CrewArrived && _expectedCrewArrival is { } arrival && _timing.CurTime >= arrival + TimeSpan.FromSeconds(5))
            CompleteCrewArrival();
        if (_finishing || !CrewArrived || _ticker.RunLevel != GameRunLevel.InRound || FinaleEndsAt is not { } end || _timing.CurTime < end) return;
        _finishing = true;
        _roundEnd.EndRound();
    }
}
