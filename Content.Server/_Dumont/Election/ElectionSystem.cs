using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Shared._Dumont.CCVar;
using Content.Shared._Dumont.Election;
using Content.Shared.Dataset;
using Content.Shared.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Election;

public sealed partial class ElectionSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private GameTicker _ticker = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IHttpClientHolder _http = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private StationSystem _station = default!;

    private static readonly ProtoId<LocalizedDatasetPrototype> Parties = "ElectionParties";
    private static readonly ProtoId<DepartmentPrototype> Command = "Command";
    private static readonly ProtoId<JobPrototype> NpcJob = "Passenger";
    private static readonly EntProtoId FallbackPod = "SpawnPodElectionScreen";
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly DateTime Start = new(2026, 10, 4, 20, 0, 0, DateTimeKind.Utc);

    private const float TestInterval = 2f;
    private const float TestWarmup = 10f;
    private const long TestVotes = 150_000_000;

    private sealed class Slot
    {
        public string Name = string.Empty;
        public string Party = string.Empty;
        public HumanoidCharacterProfile Profile = default!;
        public string? Job;
        public bool Npc;
        public string? Number; // real candidate behind the persona, never leaves the server
        public long Votes;
        public float Percent;
        public ElectionOutcome Outcome;
    }

    private readonly List<Slot> _joined = new();
    private readonly List<Slot> _slots = new();
    private bool _selected;
    private bool _standbySent;
    private bool _delivered;
    private bool _checked;
    private bool _cancelled;

    private ElectionResult? _latest;
    private bool _fetching;
    private bool _finalPending;
    private TimeSpan _nextPoll;

    private TimeSpan? _testStart;
    private float[]? _testWeights;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<ElectionScreenComponent, ActivateInWorldEvent>(OnActivate);

        Subs.CVar(_cfg, DumontCVars.ElectionTest, _ => ResetData());
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_cfg.GetCVar(DumontCVars.ElectionEnabled) || _ticker.RunLevel != GameRunLevel.InRound)
            return;

        var elapsed = Elapsed();
        if (elapsed < 0f)
            return;

        if (_cancelled)
            return;

        // nothing moves until this round has heard from the TSE at least once
        if (_checked)
        {
            if (!_standbySent)
            {
                _standbySent = true;
                Broadcast();
            }

            if (!_delivered && elapsed >= _cfg.GetCVar(DumontCVars.ElectionDelivery))
                Deliver();

            if (!_selected && elapsed >= _cfg.GetCVar(DumontCVars.ElectionDelay))
                SelectCandidates();

            if (_selected && _finalPending)
            {
                _finalPending = false;
                AnnounceFinal();
            }
        }

        if (_fetching || _timing.RealTime < _nextPoll)
            return;

        if (_cfg.GetCVar(DumontCVars.ElectionTest))
        {
            _nextPoll = _timing.RealTime + TimeSpan.FromSeconds(TestInterval);
            OnResult(Simulate());
            return;
        }

        _nextPoll = _timing.RealTime + TimeSpan.FromSeconds(_cfg.GetCVar(DumontCVars.ElectionInterval));
        Fetch(_cfg.GetCVar(DumontCVars.ElectionUrl));
    }

    private async void Fetch(string url)
    {
        _fetching = true;
        try
        {
            using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var json = await _http.Client.GetStringAsync(url, cancel.Token);
            OnResult(ElectionResult.Parse(json));
        }
        catch (Exception e)
        {
            Log.Warning($"Could not fetch election results, keeping the last ones: {e.Message}");
        }
        finally
        {
            _fetching = false;
        }
    }

    private void OnResult(ElectionResult result)
    {
        var previous = _latest?.Phase;
        _latest = result;

        // a round that starts with the count already over sits this one out
        if (!_checked)
        {
            _checked = true;
            _cancelled = result.Phase is ElectionPhase.SecondRound or ElectionPhase.Elected;
        }

        if (_cancelled)
            return;

        // only announce if we saw it happen, otherwise every restart would announce it again
        if (previous is ElectionPhase.Waiting or ElectionPhase.Counting
            && result.Phase is ElectionPhase.SecondRound or ElectionPhase.Elected)
        {
            _finalPending = true;
        }

        if (!_selected)
            return;

        Apply();
        Broadcast();
    }

    private void Apply()
    {
        if (_latest == null)
            return;

        foreach (var candidate in _latest.Candidates)
        {
            var slot = _slots.Find(s => s.Number == candidate.Number);
            if (slot == null)
            {
                var free = _slots.Where(s => s.Number == null).ToList();
                if (free.Count > 0)
                {
                    slot = _random.Pick(free);
                }
                else
                {
                    slot = MakeNpc();
                    _slots.Add(slot);
                }

                slot.Number = candidate.Number;
            }

            slot.Votes = candidate.Votes;
            slot.Percent = candidate.Percent;
            slot.Outcome = candidate.Outcome;
        }
    }

    private void SelectCandidates()
    {
        _selected = true;

        _random.Shuffle(_joined);
        var crew = _joined.OrderByDescending(p => IsCommand(p.Job)).ToList();
        var count = _latest?.Candidates.Count ?? _cfg.GetCVar(DumontCVars.ElectionCandidates);

        for (var i = 0; i < count; i++)
        {
            _slots.Add(i < crew.Count ? crew[i] : MakeNpc());
        }

        var parties = _proto.Index(Parties).Values.Select(id => Loc.GetString(id)).ToList();
        _random.Shuffle(parties);
        for (var i = 0; i < _slots.Count; i++)
        {
            _slots[i].Party = parties[i % parties.Count];
        }

        _joined.Clear();
        Apply();
        Broadcast();

        var list = new StringBuilder();
        foreach (var slot in _slots)
        {
            list.Append('\n');
            list.Append(Loc.GetString("election-announcement-candidate", ("name", slot.Name), ("party", slot.Party)));
        }

        _chat.DispatchGlobalAnnouncement(Loc.GetString("election-announcement-start", ("candidates", list.ToString())),
            colorOverride: Color.Gold);
    }

    private void AnnounceFinal()
    {
        var elected = _slots.Find(s => s.Outcome == ElectionOutcome.Elected);
        if (elected != null)
        {
            _chat.DispatchGlobalAnnouncement(Loc.GetString("election-announcement-elected",
                    ("name", elected.Name),
                    ("party", elected.Party),
                    ("percent", elected.Percent.ToString("0.00", Culture))),
                colorOverride: Color.Gold);
            return;
        }

        var runoff = _slots.Where(s => s.Outcome == ElectionOutcome.SecondRound)
            .OrderByDescending(s => s.Votes)
            .ToList();
        if (runoff.Count < 2)
            return;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("election-announcement-second-round",
                ("first", runoff[0].Name),
                ("firstParty", runoff[0].Party),
                ("second", runoff[1].Name),
                ("secondParty", runoff[1].Party)),
            colorOverride: Color.Gold);
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        AddPersona(new Slot
        {
            Name = Name(args.Mob),
            Profile = args.Profile,
            Job = args.JobId,
        });
    }

    /// <summary>
    /// temporary, for testing: pretends that many people joined the round
    /// </summary>
    public void AddFakeCrew(int amount)
    {
        var jobs = _proto.EnumeratePrototypes<JobPrototype>().Where(j => j.SetPreference).ToList();
        for (var i = 0; i < amount; i++)
        {
            var profile = HumanoidCharacterProfile.Random();
            AddPersona(new Slot
            {
                Name = profile.Name,
                Profile = profile,
                Job = _random.Pick(jobs).ID,
            });
        }
    }

    private void AddPersona(Slot persona)
    {
        if (!_selected)
        {
            _joined.Add(persona);
            return;
        }

        // late arrivals take over an npc slot, party and votes stay with the slot
        var slot = _slots.Find(s => s.Npc);
        if (slot == null)
            return;

        slot.Name = persona.Name;
        slot.Profile = persona.Profile;
        slot.Job = persona.Job;
        slot.Npc = false;
        Broadcast();
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _joined.Clear();
        _slots.Clear();
        _selected = false;
        _standbySent = false;
        _delivered = false;
        _checked = false;
        _cancelled = false;
        _nextPoll = TimeSpan.Zero;
    }

    private void ResetData()
    {
        _latest = null;
        _finalPending = false;
        _testStart = null;
        _testWeights = null;
        _nextPoll = TimeSpan.Zero;
        _checked = false;
        _cancelled = false;

        foreach (var slot in _slots)
        {
            slot.Number = null;
            slot.Votes = 0;
            slot.Percent = 0f;
            slot.Outcome = ElectionOutcome.None;
        }

        Broadcast();
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus == SessionStatus.InGame)
            RaiseNetworkEvent(BuildState(), args.Session);
    }

    private void OnActivate(Entity<ElectionScreenComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        args.Handled = true;
        ent.Comp.Mode = ent.Comp.Mode == ElectionScreenMode.Scrolling
            ? ElectionScreenMode.Top3
            : ElectionScreenMode.Scrolling;
        Dirty(ent);
    }

    // seconds since the round started or the polls closed, whichever came last
    private float Elapsed()
    {
        var round = (float) _ticker.RoundDuration().TotalSeconds;
        if (_cfg.GetCVar(DumontCVars.ElectionTest))
            return round;

        return Math.Min(round, (float) (DateTime.UtcNow - Start).TotalSeconds);
    }

    private void Deliver()
    {
        _delivered = true;

        var places = new List<string>();
        var query = EntityQueryEnumerator<ElectionScreenSpawnerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spawner, out var xform))
        {
            if (DropPod(uid, xform, spawner.Prototype, out var place))
                places.Add(place);
        }

        if (places.Count == 0)
        {
            var beacons = new List<EntityUid>();
            var beaconQuery = EntityQueryEnumerator<NavMapBeaconComponent, TransformComponent>();
            while (beaconQuery.MoveNext(out var uid, out var beacon, out _))
            {
                if (beacon.Enabled && beacon.Text != null && _station.GetOwningStation(uid) != null)
                    beacons.Add(uid);
            }

            if (beacons.Count > 0)
            {
                var uid = _random.Pick(beacons);
                if (DropPod(uid, Transform(uid), FallbackPod, out var place))
                    places.Add(place);
            }
        }

        if (places.Count == 0)
            return;

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("election-announcement-delivery", ("location", string.Join(", ", places))),
            colorOverride: Color.Gold);
    }

    private bool DropPod(EntityUid beacon, TransformComponent xform, EntProtoId pod, out string place)
    {
        place = string.Empty;
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        // the beacon usually sits under a table or chair, so look around for a free tile
        var origin = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var tile = origin;
        for (var i = 0; i < 9; i++)
        {
            var candidate = origin + new Vector2i(i % 3 - 1, i / 3 - 1);
            if (IsBlocked(gridUid, grid, candidate))
                continue;

            tile = candidate;
            break;
        }

        Spawn(pod, _map.GridTileToLocal(gridUid, grid, tile));
        place = CompOrNull<NavMapBeaconComponent>(beacon)?.Text ?? Name(beacon);
        return true;
    }

    private bool IsBlocked(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        if (!_map.TryGetTileRef(gridUid, grid, tile, out var tileRef) || tileRef.Tile.IsEmpty)
            return true;

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile);
        while (anchored.MoveNext(out var uid))
        {
            if (TryComp<PhysicsComponent>(uid, out var physics) && physics.CanCollide && physics.Hard)
                return true;
        }

        return false;
    }

    private bool IsCommand(string? job)
    {
        return job != null
               && _proto.TryIndex(Command, out var department)
               && department.Roles.Contains(job);
    }

    private Slot MakeNpc()
    {
        var profile = HumanoidCharacterProfile.Random();
        return new Slot
        {
            Name = profile.Name,
            Profile = profile,
            Job = NpcJob,
            Npc = true,
        };
    }

    private ElectionStateEvent BuildState()
    {
        var state = new ElectionStateEvent();
        if (!_selected)
        {
            state.Phase = ElectionPhase.Standby;
            state.StartsAt = _timing.CurTime + TimeSpan.FromSeconds(_cfg.GetCVar(DumontCVars.ElectionDelay) - Elapsed());
            return state;
        }

        state.Phase = _latest?.Phase ?? ElectionPhase.Waiting;
        state.Sections = _latest?.Sections ?? 0f;
        state.LastUpdate = _latest?.LastUpdate ?? string.Empty;

        // once the real list is in, slots without a candidate are dropped
        var shown = _latest == null ? _slots : _slots.Where(s => s.Number != null);
        foreach (var slot in shown.OrderByDescending(s => s.Votes))
        {
            state.Candidates.Add(new ElectionCandidate
            {
                Name = slot.Name,
                Party = slot.Party,
                Profile = slot.Profile,
                Job = slot.Job,
                Votes = slot.Votes,
                Percent = slot.Percent,
                Outcome = slot.Outcome,
            });
        }

        return state;
    }

    private void Broadcast()
    {
        RaiseNetworkEvent(BuildState());
    }

    private ElectionResult Simulate()
    {
        _testStart ??= _timing.RealTime;
        if (_testWeights == null)
        {
            _testWeights = new float[_cfg.GetCVar(DumontCVars.ElectionCandidates)];
            for (var i = 0; i < _testWeights.Length; i++)
            {
                var roll = _random.NextFloat();
                _testWeights[i] = roll * roll * roll + 0.01f;
            }
        }

        var elapsed = (float) (_timing.RealTime - _testStart.Value).TotalSeconds;
        var progress = Math.Clamp((elapsed - TestWarmup) / _cfg.GetCVar(DumontCVars.ElectionTestDuration), 0f, 1f);
        var sum = _testWeights.Sum();
        var top = _testWeights.OrderByDescending(w => w).Take(2).ToList();
        var done = progress >= 1f;
        var majority = top[0] / sum > 0.5f;

        var candidates = new List<TseCandidate>();
        for (var i = 0; i < _testWeights.Length; i++)
        {
            var share = _testWeights[i] / sum;
            var outcome = ElectionOutcome.None;
            if (done && majority && _testWeights[i] == top[0])
                outcome = ElectionOutcome.Elected;
            else if (done && !majority && top.Contains(_testWeights[i]))
                outcome = ElectionOutcome.SecondRound;

            candidates.Add(new TseCandidate($"T{i}", (long) (TestVotes * progress * share), progress > 0f ? share * 100f : 0f, outcome));
        }

        return new ElectionResult(candidates, progress * 100f, DateTime.Now.ToString("HH:mm"));
    }
}
