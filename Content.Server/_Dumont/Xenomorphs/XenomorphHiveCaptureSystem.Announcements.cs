using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.GameTicking;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveCaptureSystem
{
    [Dependency] private Content.Server.Nuke.NukeCodePaperSystem _codes = default!;
    [Dependency] private Content.Server.Communications.CommunicationsConsoleSystem _comms = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private Content.Server.Chat.Managers.IChatManager _hiveChat = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private ServerGlobalSoundSystem _music = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    private TimeSpan? _infestationStarted;
    private TimeSpan _nextAnnouncementCheck;
    private bool _unlockAnnounced;
    private TimeSpan? _contactAnnouncementAt;
    private readonly HashSet<EntityUid> _readyClusters = new();
    private readonly HashSet<EntityUid> _knownCores = new();
    private readonly Dictionary<EntityUid, TimeSpan> _captureSounds = new();

    public bool CaptureUnlocked => _infestationStarted is { } start && _timing.CurTime >= start + TimeSpan.FromMinutes(20);

    public void StartInfestation() => _infestationStarted ??= _timing.CurTime;

    private void InitializeAnnouncements()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCaptureRoundRestart);
        SubscribeLocalEvent<XenomorphHiveClusterComponent, DestructionEventArgs>(OnClusterDestroyed);
        SubscribeLocalEvent<XenomorphHiveCoreComponent, DestructionEventArgs>(OnCoreDestroyed);
        SubscribeLocalEvent<XenomorphHivePylonComponent, DestructionEventArgs>(OnPylonDestroyed);
        SubscribeLocalEvent<XenomorphHivePylonComponent, XenomorphHivePylonCapturedEvent>(OnPylonCaptured);
    }

    private void OnCaptureRoundRestart(RoundRestartCleanupEvent args)
    {
        _infestationStarted = null;
        _unlockAnnounced = false;
        _contactAnnouncementAt = null;
        _nextAnnouncementCheck = default;
        _readyClusters.Clear();
        _knownCores.Clear();
        _captureSounds.Clear();
    }

    public void AnnounceHive(string message, SoundSpecifier? sound = null)
    {
        var query = EntityQueryEnumerator<ActorComponent, XenomorphLineageComponent, XenomorphComponent>();
        while (query.MoveNext(out _, out var actor, out _, out _))
        {
            RaiseNetworkEvent(new XenomorphQueenAnnouncementEvent(message), actor.PlayerSession);
            _hiveChat.ChatMessageToOne(ChatChannel.Local, message, FormattedMessage.EscapeText(message), default, false,
                actor.PlayerSession.Channel, Color.FromHex("#b56dd6"), recordReplay: true, hidePopup: true);
        }
        PlayHiveCommandSound(sound);
    }

    public void PlayHiveCommandSound(SoundSpecifier? sound = null)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<ActorComponent, XenomorphLineageComponent, XenomorphComponent>();
        while (query.MoveNext(out _, out var actor, out _, out _)) filter.AddPlayer(actor.PlayerSession);
        _audio.PlayGlobal(sound ?? new SoundCollectionSpecifier("DumontXenomorphQueenCommand", AudioParams.Default.WithVolume(-6)), filter, true);
    }

    private string Location(EntityUid uid) => FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((uid, Transform(uid))));

    private void OnClusterDestroyed(Entity<XenomorphHiveClusterComponent> ent, ref DestructionEventArgs args)
        => AnnounceHive(Loc.GetString("xenomorph-hive-cluster-destroyed", ("location", Location(ent))));

    private void OnCoreDestroyed(Entity<XenomorphHiveCoreComponent> ent, ref DestructionEventArgs args)
        => AnnounceHive(Loc.GetString("xenomorph-hive-core-destroyed", ("location", Location(ent))));

    private void OnPylonDestroyed(Entity<XenomorphHivePylonComponent> ent, ref DestructionEventArgs args)
        => AnnounceHive(Loc.GetString("xenomorph-hive-pylon-destroyed", ("location", Location(ent))));

    private void OnPylonCaptured(Entity<XenomorphHivePylonComponent> ent, ref XenomorphHivePylonCapturedEvent args)
    {
        _contactAnnouncementAt = _timing.CurTime + TimeSpan.FromSeconds(20);
        var location = Location(args.Target);
        if (_stations.GetOwningStation(args.Target) is { } station && _codes.TryGetStationNukeCodes(station, out var codes))
            _comms.ReceiveNuclearCodes(station, codes);
        var hiveMessage = Loc.GetString("xenomorph-hive-pylon-captured", ("location", location));
        if (EntityManager.System<XenomorphHiveRanksSystem>().UnlockRoyalGuardSlot())
            hiveMessage += "\n" + Loc.GetString("xenomorph-royal-guard-unlocked");
        AnnounceHive(hiveMessage);
        _chat.DispatchGlobalAnnouncement(Loc.GetString("xenomorph-hive-pylon-cc", ("location", location)),
            Loc.GetString("xenomorph-hive-pylon-cc-sender"), colorOverride: Color.Red);
        if (EntityManager.System<XenomorphHiveObjectivesSystem>().HijackingQueen != null) return;
        var sound = new SoundPathSpecifier("/Audio/_Dumont/Eventos/Vendetta.ogg", AudioParams.Default.WithVolume(-4));
        _music.StopStationEventMusic(args.Target, StationEventMusicType.Xenomorph);
        _music.DispatchStationEventMusic(args.Target, sound, StationEventMusicType.Xenomorph, sound.Params);
    }

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextAnnouncementCheck) return;
        _nextAnnouncementCheck = _timing.CurTime + TimeSpan.FromSeconds(1);
        if (_contactAnnouncementAt is { } contactAt && _timing.CurTime >= contactAt)
        {
            _contactAnnouncementAt = null;
            AnnounceHive(Loc.GetString("xenomorph-hive-contact-unlocked"));
        }
        if (_infestationStarted == null)
        {
            var xenos = EntityQueryEnumerator<XenomorphLineageComponent, XenomorphComponent>();
            if (xenos.MoveNext(out _, out _, out _)) StartInfestation();
        }
        var cores = EntityQueryEnumerator<XenomorphHiveCoreComponent>();
        while (cores.MoveNext(out var uid, out _))
            if (!TerminatingOrDeleted(uid) && _knownCores.Add(uid))
                AnnounceHive(Loc.GetString("xenomorph-hive-core-created", ("location", Location(uid))));
        if (CaptureUnlocked && !_unlockAnnounced)
        {
            _unlockAnnounced = true;
            AnnounceHive(Loc.GetString("xenomorph-hive-capture-unlocked"));
        }
        var clusters = EntityQueryEnumerator<XenomorphHiveClusterComponent>();
        while (clusters.MoveNext(out var uid, out var cluster))
        {
            if (TerminatingOrDeleted(uid)) continue;
            if (CaptureUnlocked && PylonSlotAvailable(uid) && FindCaptureTarget(uid) != null)
            {
                if (_readyClusters.Add(uid))
                    AnnounceHive(Loc.GetString("xenomorph-hive-cluster-ready", ("location", Location(uid))));
            }
            else _readyClusters.Remove(uid);
            if (cluster.CapturingUser == null) { _captureSounds.Remove(uid); continue; }
            if (_captureSounds.TryGetValue(uid, out var next) && next > _timing.CurTime) continue;
            _captureSounds[uid] = _timing.CurTime + TimeSpan.FromSeconds(2);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_resin_move1.ogg", AudioParams.Default.WithVolume(-4)), uid);
        }
    }
}
