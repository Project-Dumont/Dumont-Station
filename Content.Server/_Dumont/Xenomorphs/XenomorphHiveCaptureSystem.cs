using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using Content.Server._White.Spawners.Components;
using Content.Server.Nuke;
using Content.Server.Station.Systems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Map.Components;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveCaptureSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private StationSystem _stations = default!;

    public override void Initialize()
    {
        InitializeAnnouncements();
        SubscribeLocalEvent<XenomorphHiveClusterComponent, ActivateInWorldEvent>(OnActivate, before: new[] { typeof(XenomorphHiveConstructionSystem) });
        SubscribeLocalEvent<XenomorphHiveClusterComponent, GetVerbsEvent<Verb>>(OnVerbs);
        SubscribeLocalEvent<XenomorphHiveClusterComponent, XenomorphHiveCaptureDoAfterEvent>(OnCaptured);
        SubscribeLocalEvent<XenomorphHiveClusterComponent, DoAfterAttemptEvent<XenomorphHiveCaptureDoAfterEvent>>(OnAttempt);
        SubscribeLocalEvent<XenomorphHiveClusterComponent, ExaminedEvent>(OnClusterExamined);
        SubscribeLocalEvent<XenomorphHivePylonComponent, ComponentShutdown>(OnPylonRemoved);
        SubscribeLocalEvent<XenomorphHiveCapturedComponent, ExaminedEvent>(OnCapturedExamined);
    }

    private bool Ready(EntityUid user, EntityUid cluster) => HasComp<XenomorphHiveBuilderComponent>(user)
        && HasComp<XenomorphLineageComponent>(user) && _mobs.IsAlive(user) && _blocker.CanConsciouslyPerformAction(user)
        && (!TryComp<XenomorphRestComponent>(user, out var rest) || !rest.Resting)
        && _interaction.InRangeUnobstructed(user, cluster, 1.75f);

    private bool PylonSlotAvailable(EntityUid cluster)
    {
        var pylons = EntityQueryEnumerator<XenomorphHivePylonComponent>();
        while (pylons.MoveNext(out var uid, out _)) if (!TerminatingOrDeleted(uid)) return false;
        var captures = EntityQueryEnumerator<XenomorphHiveClusterComponent>();
        while (captures.MoveNext(out var uid, out var capture))
            if (uid != cluster && !TerminatingOrDeleted(uid) && capture.CapturingUser != null) return false;
        return true;
    }

    private bool ValidTarget(EntityUid cluster, EntityUid target)
    {
        if (TerminatingOrDeleted(target) || !HasComp<XenomorphHiveCaptureTargetComponent>(target)
            || Transform(target).GridUid != Transform(cluster).GridUid || !Transform(target).Anchored) return false;
        if (TryComp<XenomorphHiveCapturedComponent>(target, out var captured) && !TerminatingOrDeleted(captured.Pylon)) return false;
        if (TryComp<NukeComponent>(target, out var nuke))
        {
            if (nuke.OriginStation is { } origin && _stations.GetOwningStation(target) != origin) return false;
            if (nuke.OriginStation == null && nuke.OriginMapGrid is { } location && location.Item2 != Transform(target).GridUid) return false;
        }
        return true;
    }

    public bool HasWeedContact(EntityUid cluster, EntityUid target)
    {
        if (!ValidTarget(cluster, target) || !TryComp<AreaSpawnerComponent>(cluster, out var source)
            || Transform(cluster).GridUid is not { } grid || !TryComp<MapGridComponent>(grid, out var mapGrid)) return false;
        var tile = _map.TileIndicesFor(grid, mapGrid, Transform(target).Coordinates);
        foreach (var weed in source.Spawneds)
        {
            if (TerminatingOrDeleted(weed) || !HasComp<XenomorphHiveWeedComponent>(weed) || Transform(weed).GridUid != grid) continue;
            if (_map.TileIndicesFor(grid, mapGrid, Transform(weed).Coordinates) == tile) return true;
        }
        return false;
    }

    public EntityUid? FindCaptureTarget(EntityUid cluster)
    {
        if (!TryComp<AreaSpawnerComponent>(cluster, out var source) || Transform(cluster).GridUid is not { } grid
            || !TryComp<MapGridComponent>(grid, out var mapGrid)) return null;
        foreach (var weed in source.Spawneds)
        {
            if (TerminatingOrDeleted(weed) || !HasComp<XenomorphHiveWeedComponent>(weed) || Transform(weed).GridUid != grid) continue;
            foreach (var target in _map.GetAnchoredEntities((grid, mapGrid), Transform(weed).Coordinates))
                if (ValidTarget(cluster, target)) return target;
        }
        return null;
    }

    public bool TryStartCapture(Entity<XenomorphHiveClusterComponent> ent, EntityUid user)
    {
        if (!Ready(user, ent) || ent.Comp.CapturingUser != null) return false;
        if (!CaptureUnlocked)
        { _popup.PopupEntity(Loc.GetString("xenomorph-hive-capture-locked"), ent, user); return false; }
        if (!PylonSlotAvailable(ent))
        { _popup.PopupEntity(Loc.GetString("xenomorph-hive-pylon-limit"), ent, user); return false; }
        if (FindCaptureTarget(ent) is not { } target)
        { _popup.PopupEntity(Loc.GetString("xenomorph-hive-capture-no-target"), ent, user); return false; }
        ent.Comp.CapturingUser = user;
        ent.Comp.CaptureTarget = target;
        var started = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.CaptureDelay,
            new XenomorphHiveCaptureDoAfterEvent { CaptureTarget = GetNetEntity(target) }, ent, ent)
        {
            NeedHand = false, BreakOnMove = true, BreakOnDamage = true,
            BlockDuplicate = true, CancelDuplicate = false, AttemptFrequency = AttemptFrequency.EveryTick,
        });
        if (!started) { ent.Comp.CapturingUser = null; ent.Comp.CaptureTarget = null; }
        else _popup.PopupEntity(Loc.GetString("xenomorph-hive-capture-start", ("target", target)), ent, user);
        return started;
    }

    private void OnActivate(Entity<XenomorphHiveClusterComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Handled && Ready(args.User, ent)) args.Handled = TryStartCapture(ent, args.User);
    }

    private void OnVerbs(Entity<XenomorphHiveClusterComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !Ready(args.User, ent)) return;
        var user = args.User;
        args.Verbs.Add(new Verb { Text = Loc.GetString("xenomorph-hive-capture-verb"), Act = () => TryStartCapture(ent, user) });
    }

    private void OnAttempt(Entity<XenomorphHiveClusterComponent> ent, ref DoAfterAttemptEvent<XenomorphHiveCaptureDoAfterEvent> args)
    {
        if (!Ready(args.Event.User, ent) || !PylonSlotAvailable(ent) || !HasWeedContact(ent, GetEntity(args.Event.CaptureTarget))) args.Cancel();
    }

    private void OnCaptured(Entity<XenomorphHiveClusterComponent> ent, ref XenomorphHiveCaptureDoAfterEvent args)
    {
        if (args.Handled) return;
        ent.Comp.CapturingUser = null;
        ent.Comp.CaptureTarget = null;
        var target = GetEntity(args.CaptureTarget);
        if (args.Cancelled || !Ready(args.User, ent) || !PylonSlotAvailable(ent) || !HasWeedContact(ent, target)) return;
        args.Handled = true;
        var pylon = Spawn("DumontHivePylon", _transform.GetMoverCoordinates(ent));
        Comp<XenomorphHivePylonComponent>(pylon).CapturedTarget = target;
        EnsureComp<XenomorphHiveCapturedComponent>(target).Pylon = pylon;
        if (TryComp<AreaSpawnerComponent>(ent, out var oldSource) && TryComp<AreaSpawnerComponent>(pylon, out var newSource))
        {
            newSource.Spawneds.AddRange(oldSource.Spawneds);
            oldSource.Spawneds.Clear();
        }
        _popup.PopupEntity(Loc.GetString("xenomorph-hive-capture-complete", ("target", target)), ent, args.User);
        var ev = new XenomorphHivePylonCapturedEvent(target);
        RaiseLocalEvent(pylon, ref ev, true);
        QueueDel(ent);
    }

    private void OnClusterExamined(Entity<XenomorphHiveClusterComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<XenomorphLineageComponent>(args.Examiner)) return;
        args.PushMarkup(Loc.GetString("xenomorph-hive-capture-description"));
        if (CaptureUnlocked && FindCaptureTarget(ent) is { } target)
            args.PushMarkup(Loc.GetString("xenomorph-hive-capture-ready", ("target", target)));
    }

    private void OnPylonRemoved(Entity<XenomorphHivePylonComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.CapturedTarget is { } target && TryComp<XenomorphHiveCapturedComponent>(target, out var captured) && captured.Pylon == ent.Owner)
            RemCompDeferred<XenomorphHiveCapturedComponent>(target);
    }

    private void OnCapturedExamined(Entity<XenomorphHiveCapturedComponent> ent, ref ExaminedEvent args)
    {
        if (!TerminatingOrDeleted(ent.Comp.Pylon)) args.PushMarkup(Loc.GetString("xenomorph-hive-captured-target"));
    }
}
