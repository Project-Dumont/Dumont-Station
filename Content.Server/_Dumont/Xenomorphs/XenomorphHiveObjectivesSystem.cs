using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server._White.GameTicking.Rules;
using Content.Server._White.Roles;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Audio;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.GameTicking;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveObjectivesSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedObjectivesSystem _objectives = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private StationSystem _stations = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private ServerGlobalSoundSystem _music = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private EmergencyShuttleSystem _shuttle = default!;
    [Dependency] private XenomorphsRuleSystem _rules = default!;
    [Dependency] private XenomorphHiveCaptureSystem _hive = default!;
    [Dependency] private XenomorphCorruptedEvacSystem _corruptedEvac = default!;
    [Dependency] private XenomorphShuttleHijackSystem _invasion = default!;

    public bool PylonEstablished { get; private set; }
    public bool CommsCorrupted { get; private set; }
    public EntityUid? HijackingQueen { get; private set; }
    private DoAfterId? _hijackDoAfter;
    private EntityUid? _musicSource;
    private readonly HashSet<EntityUid> _pendingMinds = new();

    public override void Initialize()
    {
        InitializeResults();
        SubscribeLocalEvent<CommunicationsConsoleComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<XenomorphLineageComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<XenomorphHiveObjectiveComponent, RequirementCheckEvent>(OnRequirement);
        SubscribeLocalEvent<XenomorphHiveObjectiveComponent, ObjectiveAfterAssignEvent>(OnObjectiveAssigned);
        SubscribeLocalEvent<XenomorphHiveObjectiveComponent, ObjectiveGetProgressEvent>(OnProgress);
        SubscribeLocalEvent<XenomorphHivePylonCapturedEvent>(OnPylon);
        SubscribeLocalEvent<CommunicationsConsoleComponent, GetVerbsEvent<Verb>>(OnConsoleVerbs);
        SubscribeLocalEvent<CommunicationsConsoleComponent, ActivateInWorldEvent>(OnActivate, before: new[] { typeof(ActivatableUISystem) });
        SubscribeLocalEvent<XenomorphRoyalComponent, GetVerbsEvent<Verb>>(OnQueenVerbs);
        SubscribeLocalEvent<XenomorphCommsHijackComponent, XenomorphCommsHijackDoAfterEvent>(OnHijacked);
        SubscribeLocalEvent<XenomorphCommsHijackComponent, DoAfterAttemptEvent<XenomorphCommsHijackDoAfterEvent>>(OnAttempt);
        SubscribeLocalEvent<XenomorphCommsHijackComponent, ComponentShutdown>(OnQueenRemoved);
    }

    private void OnRestart(RoundRestartCleanupEvent args)
    {
        PylonEstablished = false;
        CommsCorrupted = false;
        HijackingQueen = null;
        _hijackDoAfter = null;
        _musicSource = null;
        _pendingMinds.Clear();
        _hadXenomorphs = false;
    }

    private void OnMindAdded(Entity<XenomorphLineageComponent> ent, ref MindAddedMessage args)
    {
        _hadXenomorphs = true;
        _pendingMinds.Add(args.Mind.Owner);
    }

    private void OnRequirement(Entity<XenomorphHiveObjectiveComponent> ent, ref RequirementCheckEvent args)
    {
        if (!HasComp<XenomorphLineageComponent>(args.Mind.OwnedEntity) || !HasComp<XenomorphComponent>(args.Mind.OwnedEntity))
            args.Cancelled = true;
    }

    private void OnObjectiveAssigned(Entity<XenomorphHiveObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
        => _objectives.SetIcon(ent, ent.Comp.Hijack
            ? new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Machines/computers.rsi"), "shuttle")
            : ent.Comp.Contact
            ? new SpriteSpecifier.Texture(new ResPath("/Textures/_Dumont/Interface/Objectives/comms.png"))
            : new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Structures/Xenos/hive_structures.rsi"), "pylon"), args.Objective);

    private void OnProgress(Entity<XenomorphHiveObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
        => args.Progress = (ent.Comp.Hijack ? _invasion.Invaded : ent.Comp.Contact ? CommsCorrupted : PylonEstablished) ? 1f : 0f;

    private void OnPylon(ref XenomorphHivePylonCapturedEvent args)
        => PylonEstablished = true;

    private bool IsQueen(EntityUid user) => HasComp<XenomorphRoyalComponent>(user)
        && HasComp<XenomorphLineageComponent>(user) && HasComp<XenomorphComponent>(user);

    private bool CanBegin(EntityUid user, Entity<CommunicationsConsoleComponent> console)
        => PylonEstablished && !_invasion.CommsLocked && !_shuttle.EmergencyShuttleArrived && !CommsCorrupted && HijackingQueen == null && IsQueen(user) && _mobs.IsAlive(user) && _blocker.CanConsciouslyPerformAction(user)
            && TryComp<XenomorphOvipositorComponent>(user, out var ovi) && ovi.Attached
            && !TerminatingOrDeleted(console) && console.Comp.CanShuttle && !console.Comp.Global
            && Transform(console).Anchored && _stations.GetOwningStation(console) != null
            && _interaction.InRangeUnobstructed(user, console.Owner, 3.5f);

    public bool TryBeginHijack(EntityUid queen, Entity<CommunicationsConsoleComponent> console)
    {
        if (!CanBegin(queen, console)) return false;
        EnsureComp<XenomorphCommsHijackComponent>(queen);
        HijackingQueen = queen;
        _musicSource = Transform(console).GridUid ?? console.Owner;
        // The console is deliberately not a DoAfter target: destroying it cannot interrupt the established link.
        var args = new DoAfterArgs(EntityManager, queen, TimeSpan.FromMinutes(3), new XenomorphCommsHijackDoAfterEvent(), queen)
        {
            NeedHand = false, RequireCanInteract = false, DistanceThreshold = null,
            BreakOnMove = false, BreakOnDamage = false, MultiplyDelay = false,
            CancelDuplicate = false, BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent,
            AttemptFrequency = AttemptFrequency.EveryTick,
        };
        if (!_doAfter.TryStartDoAfter(args, out _hijackDoAfter))
        {
            HijackingQueen = null;
            _musicSource = null;
            return false;
        }
        _invasion.OnCommsHijackStarted();
        _chat.DispatchGlobalAnnouncement(Loc.GetString("xenomorph-comms-hijack-alert"), "S.A.A.E.", colorOverride: Color.Red);
        _hive.AnnounceHive(Loc.GetString("xenomorph-comms-hijack-started"));
        _music.StopStationEventMusic(_musicSource.Value, StationEventMusicType.Xenomorph);
        var sound = new SoundPathSpecifier("/Audio/_RMC14/Hijack/Gatecrashers.ogg", AudioParams.Default.WithVolume(-4));
        _music.DispatchStationEventMusic(_musicSource.Value, sound, StationEventMusicType.Xenomorph, sound.Params);
        return true;
    }

    public bool CancelHijack(EntityUid queen)
    {
        if (HijackingQueen != queen || _hijackDoAfter == null) return false;
        _doAfter.Cancel(_hijackDoAfter);
        return true;
    }

    private void OnInteractHand(Entity<CommunicationsConsoleComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !IsQueen(args.User))
            return;

        args.Handled = true;
        InteractWithConsole(args.User, ent);
    }

    private void InteractWithConsole(EntityUid queen, Entity<CommunicationsConsoleComponent> console)
    {
        if (HijackingQueen == queen)
        {
            CancelHijack(queen);
            return;
        }

        if (_invasion.CommsLocked || _shuttle.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-comms-hijack-evac-docked"), queen, queen);
            return;
        }
        if (!PylonEstablished)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-comms-hijack-requires-pylon"), queen, queen);
            return;
        }
        if (!TryBeginHijack(queen, console))
            _popup.PopupEntity(Loc.GetString("xenomorph-comms-hijack-requirements"), queen, queen);
    }

    private void OnActivate(Entity<CommunicationsConsoleComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !IsQueen(args.User)) return;
        args.Handled = true;
        InteractWithConsole(args.User, ent);
    }

    private void OnConsoleVerbs(Entity<CommunicationsConsoleComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !CanBegin(args.User, ent)) return;
        var user = args.User;
        args.Verbs.Add(new Verb { Text = Loc.GetString("xenomorph-comms-hijack-verb"), Act = () => TryBeginHijack(user, ent) });
    }

    private void OnQueenVerbs(Entity<XenomorphRoyalComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (args.User != ent.Owner || HijackingQueen != ent.Owner) return;
        args.Verbs.Add(new Verb { Text = Loc.GetString("xenomorph-comms-hijack-cancel"), Act = () => CancelHijack(ent) });
    }

    private bool CanContinue(EntityUid queen) => !_invasion.CommsLocked && !TerminatingOrDeleted(queen) && !_mobs.IsDead(queen)
        && TryComp<XenomorphOvipositorComponent>(queen, out var ovi) && ovi.Attached;

    private void OnAttempt(Entity<XenomorphCommsHijackComponent> ent, ref DoAfterAttemptEvent<XenomorphCommsHijackDoAfterEvent> args)
    {
        if (HijackingQueen != ent.Owner || !CanContinue(ent)) args.Cancel();
    }

    private void OnQueenRemoved(Entity<XenomorphCommsHijackComponent> ent, ref ComponentShutdown args)
    {
        if (HijackingQueen == ent.Owner) CancelHijack(ent);
    }

    private void OnHijacked(Entity<XenomorphCommsHijackComponent> ent, ref XenomorphCommsHijackDoAfterEvent args)
    {
        if (args.Handled || HijackingQueen != ent.Owner) return;
        args.Handled = true;
        if (_musicSource is { } source && !TerminatingOrDeleted(source))
            _music.StopStationEventMusic(source, StationEventMusicType.Xenomorph);
        HijackingQueen = null;
        _hijackDoAfter = null;
        _musicSource = null;
        if (args.Cancelled || !CanContinue(ent))
        {
            _hive.AnnounceHive(Loc.GetString("xenomorph-comms-hijack-interrupted"));
            return;
        }
        CommsCorrupted = true;
        _rules.CompleteHiveContact();
        _hive.AnnounceHive(Loc.GetString("xenomorph-comms-hijack-complete"));
        _shuttle.SetDockTimeOverride(30f);
        if (!_shuttle.EmergencyShuttleArrived && !_shuttle.ShuttlesLeft)
        {
            _corruptedEvac.EnableCorruption();
            _roundEnd.RequestRoundEnd(TimeSpan.FromMinutes(2), checkCooldown: false,
                text: "xenomorph-comms-hijack-evacuation", name: "xenomorph-comms-hijack-unknown", replaceExisting: true, allowRecall: false,
                announcementColor: Color.FromHex("#b56dd6"),
                announcementSound: new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg", AudioParams.Default.WithVolume(-7)));
        }
    }

    public void RefreshHiveObjectives()
    {
        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var uid, out var mind))
            if (HasComp<XenomorphLineageComponent>(mind.OwnedEntity) && HasComp<XenomorphComponent>(mind.OwnedEntity))
                _pendingMinds.Add(uid);
    }

    public override void Update(float frameTime)
    {
        foreach (var mindId in _pendingMinds)
        {
            if (!TryComp<MindComponent>(mindId, out var mind) || !HasComp<XenomorphLineageComponent>(mind.OwnedEntity)
                || !HasComp<XenomorphComponent>(mind.OwnedEntity)) continue;
            if (!_roles.MindHasRole<XenomorphRoleComponent>(mindId)) _roles.MindAddRole(mindId, "MindRoleXenomorph", mind, silent: true);
            if (_invasion.CommsLocked)
                for (var i = mind.Objectives.Count - 1; i >= 0; i--)
                    if (MetaData(mind.Objectives[i]).EntityPrototype?.ID == "XenomorphCorruptCommsObjective")
                        _mind.TryRemoveObjective(mindId, mind, i);
            foreach (var objective in new[] { "XenomorphSurviveObjective", "XenomorphEstablishPylonObjective",
                _invasion.CommsLocked ? "XenomorphHijackShuttleObjective" : "XenomorphCorruptCommsObjective" })
                if (!mind.Objectives.Any(uid => MetaData(uid).EntityPrototype?.ID == objective)) _mind.TryAddObjective(mindId, mind, objective);
        }
        _pendingMinds.Clear();
    }
}
