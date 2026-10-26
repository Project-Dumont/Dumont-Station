using Content.Shared.Shuttles.Components;
using Content.Shared.DoAfter;
using Content.Server.ADT.Shuttles;
using Robust.Shared.Random;
using Content.Shared._CMU14.Xenomorphs;
using System.Numerics;
using System.Linq;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._NF.Shuttles;
using Content.Shared.ActionBlocker;
using Content.Shared.Audio;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphShuttleHijackSystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ShuttleSystem _shuttles = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private ExplosionSystem _explosions = default!;

    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private DropPodConsoleSystem _dropPods = default!;
    [Dependency] private IRobustRandom _random = default!;

    public EntityUid? HijackingQueen { get; private set; }
    private EntityUid? _hijackConsole;
    private DoAfterId? _shuttleDoAfter;
    private TimeSpan? _crashWarningAt;
    private TimeSpan? _quartersAt;
    private TimeSpan? _nextExplosion;
    private readonly List<EntityCoordinates> _fireLocations = new();

    private void ResetHijackInteraction()
    {
        HijackingQueen = null;
        _hijackConsole = null;
        _shuttleDoAfter = null;
        _crashWarningAt = null;
        _quartersAt = null;
        _nextExplosion = null;
        _fireLocations.Clear();
        _pendingExplosions.Clear();
    }

    private void InitializeInteraction()
    {
        SubscribeLocalEvent<XenomorphRoyalComponent, AttackAttemptEvent>(OnConsoleAttack);
        SubscribeLocalEvent<XenomorphShuttleHijackingComponent, ComponentShutdown>(OnHijackingQueenRemoved);
        SubscribeLocalEvent<XenomorphRoyalComponent, XenomorphShuttleHijackDoAfterEvent>(OnHijackFinished);
        SubscribeLocalEvent<XenomorphRoyalComponent, DoAfterAttemptEvent<XenomorphShuttleHijackDoAfterEvent>>(OnHijackAttempt);
        SubscribeLocalEvent<ShuttleConsoleComponent, InteractHandEvent>(OnConsoleHand);
        SubscribeLocalEvent<ShuttleConsoleComponent, ActivateInWorldEvent>(OnConsoleActivate, before: new[] { typeof(ActivatableUISystem) });
        SubscribeLocalEvent<ShuttleConsoleComponent, GetVerbsEvent<Verb>>(OnConsoleVerbs);
    }

    private bool IsQueen(EntityUid uid) => HasComp<XenomorphRoyalComponent>(uid)
        && HasComp<XenomorphLineageComponent>(uid);

    private void OnConsoleAttack(Entity<XenomorphRoyalComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Cancelled || args.Target is not { } target || !TryComp<ShuttleConsoleComponent>(target, out var console)
            || !TryComp<XenomorphOvipositorComponent>(ent, out var ovi) || !ovi.Attached) return;
        // A console click must not become a claw strike when combat mode is selected.
        args.Cancel();
        TryHijack(ent, (target, console));
    }

    private void OnConsoleHand(Entity<ShuttleConsoleComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !IsQueen(args.User)) return;
        args.Handled = true;
        TryHijack(args.User, ent);
    }

    private void OnConsoleActivate(Entity<ShuttleConsoleComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !IsQueen(args.User)) return;
        args.Handled = true;
        TryHijack(args.User, ent);
    }

    private void OnConsoleVerbs(Entity<ShuttleConsoleComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !IsQueen(args.User) || !CommsLocked) return;
        var user = args.User;
        args.Verbs.Add(new Verb { Text = Loc.GetString("xenomorph-shuttle-hijack-verb"), Act = () => TryHijack(user, ent) });
    }

    public bool TryHijack(EntityUid queen, Entity<ShuttleConsoleComponent> console)
    {
        if (HijackingQueen == queen)
        {
            _doAfter.Cancel(_shuttleDoAfter);
            return true;
        }
        string? failure = null;
        var grid = Transform(console).GridUid;
        if (WindowClosed) failure = "xenomorph-shuttle-hijack-window-lost";
        else if (!CommsLocked || _objectives.CommsCorrupted) failure = "xenomorph-shuttle-hijack-unavailable";
        else if (HijackedShuttle != null || HijackingQueen != null) failure = "xenomorph-shuttle-hijack-already-started";
        else if (!IsQueen(queen) || !_mobs.IsAlive(queen) || !_blocker.CanConsciouslyPerformAction(queen)
            || !TryComp<XenomorphOvipositorComponent>(queen, out var ovi) || !ovi.Attached
            || !_interaction.InRangeUnobstructed(queen, console.Owner, 3.5f)
            || Transform(queen).GridUid != grid || !Transform(console).Anchored
            || grid == null || !HasComp<ShuttleComponent>(grid) || !HasComp<MapGridComponent>(grid)
            || HasComp<FTLComponent>(grid) || grid == _evac
            || !Exists(_centcomm) || !Exists(_centcommMap)
            || Transform(grid!.Value).MapUid == _centcommMap)
            failure = "xenomorph-shuttle-hijack-requirements";
        if (failure != null)
        {
            _popup.PopupEntity(Loc.GetString(failure), queen, queen);
            return false;
        }

        EnsureComp<XenomorphShuttleHijackingComponent>(queen);
        var args = new DoAfterArgs(EntityManager, queen, TimeSpan.FromSeconds(120), new XenomorphShuttleHijackDoAfterEvent(), queen, target: console.Owner)
        {
            NeedHand = false, RequireCanInteract = false, DistanceThreshold = null,
            BreakOnMove = true, BreakOnDamage = false, MultiplyDelay = false,
            CancelDuplicate = false, BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent,
            AttemptFrequency = AttemptFrequency.EveryTick,
        };
        // TryStartDoAfter raises the first attempt check synchronously.
        HijackingQueen = queen;
        _hijackConsole = console;
        if (!_doAfter.TryStartDoAfter(args, out _shuttleDoAfter))
        {
            HijackingQueen = null;
            _hijackConsole = null;
            return false;
        }
        _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-channel"));
        return true;
    }

    private void OnHijackingQueenRemoved(Entity<XenomorphShuttleHijackingComponent> ent, ref ComponentShutdown args)
    {
        if (HijackingQueen != ent.Owner) return;
        _doAfter.Cancel(_shuttleDoAfter);
        HijackingQueen = null;
        _hijackConsole = null;
        _shuttleDoAfter = null;
        if (CrewArrived && HijackedShuttle == null) FinaleEndsAt = _timing.CurTime;
    }

    private bool CanContinueHijack(EntityUid queen)
        => Exists(queen) && IsQueen(queen) && _mobs.IsAlive(queen)
        && TryComp<XenomorphOvipositorComponent>(queen, out var ovi) && ovi.Attached
        && _hijackConsole is { } console && Exists(console) && Transform(console).Anchored
        && Transform(queen).GridUid is { } grid && Transform(console).GridUid == grid
        && HasComp<ShuttleComponent>(grid) && !HasComp<FTLComponent>(grid)
        && _interaction.InRangeUnobstructed(queen, console, 3.5f)
        && Exists(_centcomm) && Exists(_centcommMap);

    private void OnHijackAttempt(Entity<XenomorphRoyalComponent> ent, ref DoAfterAttemptEvent<XenomorphShuttleHijackDoAfterEvent> args)
    {
        if (HijackingQueen != ent.Owner || !CanContinueHijack(ent)) args.Cancel();
    }

    private void OnHijackFinished(Entity<XenomorphRoyalComponent> ent, ref XenomorphShuttleHijackDoAfterEvent args)
    {
        if (args.Handled || HijackingQueen != ent.Owner) return;
        args.Handled = true;
        var valid = !args.Cancelled && CanContinueHijack(ent);
        var shuttle = valid ? Transform(ent).GridUid : null;
        HijackingQueen = null;
        _hijackConsole = null;
        _shuttleDoAfter = null;
        if (shuttle == null || !LaunchHijackedShuttle(shuttle.Value))
        {
            _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-interrupted"));
            // A cancelled reserved attempt must not leave the native round-end timer vetoed forever.
            if (CrewArrived && HijackedShuttle == null) FinaleEndsAt = _timing.CurTime;
        }
    }

    private bool LaunchHijackedShuttle(EntityUid shuttle)
    {
        // The window was reserved when the do-after began, before the crew reached CC.
        // Hijacking supplies the navigation link even for local shuttles without an FTL drive.
        EnsureComp<FTLDriveComponent>(shuttle);
        EnsureComp<FTLSmashImmuneComponent>(_centcomm!.Value);
        var landing = GetCrashLanding(shuttle, _centcomm.Value);
        _shuttles.FTLToCoordinates(shuttle, Comp<ShuttleComponent>(shuttle), landing.Coordinates, landing.Rotation,
            startupTime: 0, hyperspaceTime: 180);
        if (!HasComp<FTLComponent>(shuttle)) return false;
        HijackedShuttle = shuttle;
        EnsureComp<XenomorphHijackedShuttleComponent>(shuttle);
        _quartersAt = _timing.CurTime + TimeSpan.FromSeconds(10);
        _crashWarningAt = _timing.CurTime + TimeSpan.FromSeconds(157);
        AnnounceInvasion("xenomorph-invasion-unscheduled");
        _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-departing"));
        return true;
    }

    private void OnInvasionArrived(Entity<XenomorphHijackedShuttleComponent> ent, ref FTLCompletedEvent args)
    {
        if (HijackedShuttle != ent.Owner || Invaded || args.MapUid != _centcommMap) return;
        // Reaching the destination with an empty shuttle is not an invasion.
        var xenos = EntityQueryEnumerator<Content.Shared._White.Xenomorphs.Xenomorph.XenomorphComponent>();
        var occupants = false;
        while (xenos.MoveNext(out var uid, out _))
        {
            if (!_mobs.IsDead(uid) && Transform(uid).GridUid == ent.Owner) { occupants = true; break; }
        }
        if (!occupants) return;
        _shuttles.Disable(ent);
        ClearCrashFootprint(ent);
        Invaded = true;
        ExtendFinale(_timing.CurTime + TimeSpan.FromMinutes(5));
        _rules.CompleteShuttleInvasion();
        _hive.AnnounceHive(Loc.GetString("xenomorph-shuttle-hijack-arrived"));
        var sound = new SoundPathSpecifier("/Audio/_RMC14/Hijack/GigaXO_we_lose.ogg", AudioParams.Default.WithVolume(-4));
        RaiseNetworkEvent(new StopStationEventMusic(StationEventMusicType.Xenomorph), Filter.Broadcast());
        RaiseNetworkEvent(new StationEventMusicEvent(_audio.ResolveSound(sound), StationEventMusicType.Xenomorph, sound.Params), Filter.Broadcast());
        PrepareLandingEffects();
        AnnounceInvasion("xenomorph-invasion-impact");
        // Reuse the nuclear droppod landing: tiles and children become part of CC, not overlapping grids.
        _dropPods.MergeIntoStation(ent, Comp<MapGridComponent>(ent), _centcomm!.Value,
            Comp<MapGridComponent>(_centcomm.Value), preserveMobs: true);
        PlayLandingImpact(ent);
    }

    private (EntityCoordinates Coordinates, Angle Rotation) GetCrashLanding(EntityUid shuttle, EntityUid centcomm)
    {
        var ccGrid = Comp<MapGridComponent>(centcomm);
        var shipGrid = Comp<MapGridComponent>(shuttle);
        var center = ccGrid.LocalAABB.Center;
        var doors = new List<Vector2>();
        var docks = EntityQueryEnumerator<DockingComponent, TransformComponent>();
        while (docks.MoveNext(out var uid, out _, out var transform))
        {
            if (transform.GridUid != centcomm) continue;
            if (TryComp<PriorityDockComponent>(uid, out var priority) && priority.Tag == "DockEmergency")
                doors.Add(transform.LocalPosition);
        }
        Vector2 entrance;
        if (doors.Count > 0)
            entrance = doors.Aggregate(Vector2.Zero, (sum, pos) => sum + pos) / doors.Count;
        else
            entrance = new Vector2(ccGrid.LocalAABB.Right, center.Y);
        var delta = center - entrance;
        var inward = Math.Abs(delta.X) >= Math.Abs(delta.Y)
            ? new Vector2(Math.Sign(delta.X), 0)
            : new Vector2(0, Math.Sign(delta.Y));
        var halfSize = Math.Abs(inward.X) > 0 ? shipGrid.LocalAABB.Width / 2 : shipGrid.LocalAABB.Height / 2;
        var landingCenter = entrance + inward * (Math.Min(halfSize, 8) + 3);
        var mapPosition = _transform.ToMapCoordinates(new EntityCoordinates(centcomm, landingCenter - shipGrid.LocalAABB.Center));
        // Target the map, not the grid: FTL to a grid would automatically choose a dock.
        return (new EntityCoordinates(Transform(centcomm).MapUid!.Value, mapPosition.Position), _transform.GetWorldRotation(centcomm));
    }

    private void ClearCrashFootprint(EntityUid shuttle)
    {
        var grid = Comp<MapGridComponent>(shuttle);
        var query = EntityQueryEnumerator<TransformComponent, Robust.Shared.Physics.Components.PhysicsComponent>();
        while (query.MoveNext(out var uid, out var transform, out _))
        {
            if (uid == _centcomm || transform.GridUid != _centcomm || HasComp<Content.Shared.Mobs.Components.MobStateComponent>(uid)) continue;
            var local = _transform.ToCoordinates(shuttle, _transform.GetMapCoordinates(uid));
            if (!_map.GetTileRef(shuttle, grid, local).Tile.IsEmpty) QueueDel(uid);
        }
    }

    private void AnnounceInvasion(string key)
        => _chat.DispatchGlobalAnnouncement(Loc.GetString(key), Loc.GetString("xenomorph-hive-pylon-cc-sender"), colorOverride: Color.Red);

    private void PrepareLandingEffects()
    {
        if (!TryComp<MapGridComponent>(_centcomm, out var grid)) return;
        _fireLocations.Clear();
        foreach (var tile in _map.GetAllTiles(_centcomm!.Value, grid))
        {
            if (!_turf.IsSpace(tile))
                _fireLocations.Add(_map.GridTileToLocal(_centcomm.Value, grid, tile.GridIndices));
        }
        _nextExplosion = _timing.CurTime + TimeSpan.FromSeconds(2);
    }

    private void UpdateLandingEffects()
    {
        if (_quartersAt is { } quarters && _timing.CurTime >= quarters)
        {
            _quartersAt = null;
            AnnounceInvasion("xenomorph-invasion-general-quarters");
        }
        if (!Invaded && _crashWarningAt is { } crash && _timing.CurTime >= crash)
        {
            _crashWarningAt = null;
            AnnounceInvasion("xenomorph-invasion-collision");
        }
        if (_nextExplosion == null) return;
        if (_finishing || _ticker.RunLevel != Content.Server.GameTicking.GameRunLevel.InRound || !Exists(_centcomm) || FinaleEndsAt is { } end && _timing.CurTime >= end)
        {
            _nextExplosion = null;
            foreach (var pending in _pendingExplosions)
                if (Exists(pending.Warning)) QueueDel(pending.Warning);
            _pendingExplosions.Clear();
            return;
        }
        DetonateWarnedExplosions();
        if (_timing.CurTime < _nextExplosion) return;
        _nextExplosion = _timing.CurTime + TimeSpan.FromSeconds(6);
        if (!TryComp<MapGridComponent>(_centcomm, out var grid)) return;
        while (_fireLocations.Count > 0)
        {
            var location = _random.Pick(_fireLocations);
            if (_turf.IsSpace(_map.GetTileRef(_centcomm.Value, grid, location)))
            {
                _fireLocations.Remove(location);
                continue;
            }
            var warning = Spawn("CMUHijackExplosionWarning", location);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Ambience/Objects/gas_hiss.ogg"), location);
            _pendingExplosions.Add((_timing.CurTime + TimeSpan.FromSeconds(5), location, warning));
            break;
        }
    }
}
