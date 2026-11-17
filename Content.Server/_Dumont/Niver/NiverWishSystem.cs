using Content.Server._Dumont.Curse;
using Content.Server.Chat.Managers;
using Content.Shared._Dumont.CCVar;
using Content.Shared._Dumont.Niver;
using Content.Shared.Mobs.Components;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Niver;

public sealed partial class NiverWishSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private CurseSystem _curse = default!;

    private static readonly TimeSpan WishDuration = TimeSpan.FromSeconds(15);

    private static readonly TimeSpan Grace = TimeSpan.FromSeconds(2);

    private bool _enabled;

    private readonly HashSet<NetUserId> _answered = new();
    private readonly Dictionary<NetUserId, PendingWish> _pending = new();
    private readonly List<NetUserId> _expired = new();

    private record struct PendingWish(string Name, TimeSpan Deadline);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<NiverWishResponseEvent>(OnResponse);
        Subs.CVar(_cfg, DumontCVars.NiverEnabled, value => _enabled = value, true);
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (!_enabled || args.NewStatus != SessionStatus.InGame)
            return;

        var userId = args.Session.UserId;
        if (_answered.Contains(userId) || _pending.ContainsKey(userId))
            return;

        _pending[userId] = new PendingWish(args.Session.Name, _timing.RealTime + WishDuration + Grace);
        RaiseNetworkEvent(new NiverWishRequestEvent(WishDuration), args.Session);
    }

    private void OnResponse(NiverWishResponseEvent ev, EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId;
        if (!_pending.Remove(userId, out var wish))
            return;

        _answered.Add(userId);

        if (ev.Wished)
            _chat.DispatchServerAnnouncement(Loc.GetString("niver-wish-wished", ("name", wish.Name)), Color.Gold);
        else
            Curse(userId, wish.Name);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pending.Count == 0)
            return;

        var now = _timing.RealTime;
        foreach (var (userId, wish) in _pending)
        {
            if (now >= wish.Deadline)
                _expired.Add(userId);
        }

        foreach (var userId in _expired)
        {
            var wish = _pending[userId];
            _pending.Remove(userId);
            _answered.Add(userId);
            Curse(userId, wish.Name);
        }

        _expired.Clear();
    }

    private void Curse(NetUserId userId, string name)
    {
        _curse.AddCursedUser(userId.UserId);
        _chat.DispatchServerAnnouncement(Loc.GetString("niver-wish-cursed", ("name", name)), Color.Red);

        // aslready has a body on the station
        if (_player.TryGetSessionById(userId, out var session) &&
            session.AttachedEntity is { } mob &&
            HasComp<MobStateComponent>(mob))
            _curse.TryCurse(mob);
    }
}
