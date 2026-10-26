using Content.Server._CMU14.Xenomorphs;
using Content.Server.Chat.Systems;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Shared.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphCorruptedEvacSystem
{
    [Dependency] private EmergencyShuttleSystem _emergency = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    private static readonly (string Id, Color Color)[] DistressSources =
    [
        ("syndicate", Color.Red),
        ("spider", Color.FromHex("#006400")),
        ("tizir", Color.FromHex("#FFB6C1")),
        ("ursal", Color.FromHex("#8B0000")),
        ("centcomm", Color.Gold),
        ("viper", Color.FromHex("#9D67FF")),
        ("horizon", Color.FromHex("#B8860B")),
        ("shellguard", Color.FromHex("#A52A2A")),
        ("borgs", Color.Cyan),
        ("frontier", Color.FromHex("#FFFF99")),
    ];

    private readonly List<(string Id, Color Color)> _distress = new();
    private EntityUid? _departingShuttle;
    private TimeSpan? _departureTime;
    private int _sentDistress;
    private bool _flightFinished;
    private bool _federationAnnounced;

    private void InitializeTransmissions()
    {
        SubscribeLocalEvent<EmergencyShuttleArrivedAtStationEvent>(OnStationArrival);
        SubscribeLocalEvent<XenomorphCorruptedEvacComponent, FTLStartedEvent>(OnDeparture);
        SubscribeLocalEvent<XenomorphCorruptedEvacComponent, FTLCompletedEvent>(OnArrival);
    }

    private void OnStationArrival(ref EmergencyShuttleArrivedAtStationEvent args)
    {
        if (!_enabled || _federationAnnounced || !HasComp<XenomorphCorruptedEvacComponent>(args.Shuttle))
            return;

        _federationAnnounced = true;
        _chat.DispatchGlobalAnnouncement(Loc.GetString("xenomorph-evac-federation-message"),
            Loc.GetString("xenomorph-evac-federation-sender"), colorOverride: Color.FromHex("#DAA520"));
    }

    private void OnDeparture(Entity<XenomorphCorruptedEvacComponent> ent, ref FTLStartedEvent args)
    {
        if (!_enabled || _departureTime != null)
            return;

        _departingShuttle = ent.Owner;
        _departureTime = _timing.CurTime;
        _sentDistress = 0;
        _flightFinished = false;
        _distress.Clear();
        _distress.AddRange(DistressSources);
        _random.Shuffle(_distress);
        _distress.RemoveRange(6, _distress.Count - 6);

        var sound = new SoundPathSpecifier("/Audio/_Dumont/Eventos/Destiny_of_Colonists.ogg", AudioParams.Default.WithVolume(-4));
        RaiseNetworkEvent(new StopStationEventMusic(StationEventMusicType.Xenomorph), Filter.Broadcast());
        RaiseNetworkEvent(new StationEventMusicEvent(_audio.ResolveSound(sound), StationEventMusicType.Xenomorph, sound.Params), Filter.Broadcast());
    }

    private void OnArrival(Entity<XenomorphCorruptedEvacComponent> ent, ref FTLCompletedEvent args)
    {
        if (_departingShuttle != ent.Owner || _departureTime == null || _flightFinished)
            return;

        _flightFinished = true;
    }

    public override void Update(float frameTime)
    {
        if (_departureTime is not { } departure || _flightFinished)
            return;

        while (_sentDistress < _distress.Count && _timing.CurTime >= departure + TimeSpan.FromSeconds(30 * (_sentDistress + 1)))
        {
            var source = _distress[_sentDistress++];
            _chat.DispatchGlobalAnnouncement(Loc.GetString($"xenomorph-evac-distress-{source.Id}-message"),
                Loc.GetString($"xenomorph-evac-distress-{source.Id}-sender"), colorOverride: source.Color);
        }
    }

    private void ResetTransmissions()
    {
        if (_departureTime != null)
            RaiseNetworkEvent(new StopStationEventMusic(StationEventMusicType.Xenomorph), Filter.Broadcast());
        _departingShuttle = null;
        _departureTime = null;
        _federationAnnounced = false;
        _sentDistress = 0;
        _flightFinished = false;
        _distress.Clear();
    }
}
