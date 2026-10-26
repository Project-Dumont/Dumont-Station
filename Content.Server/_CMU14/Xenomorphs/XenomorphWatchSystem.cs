using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._CMU14.Xenomorphs.Watch;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphWatchSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedEyeSystem _eye = default!;
    [Dependency] private ViewSubscriberSystem _views = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly Dictionary<EntityUid, (EntityUid Target, ICommonSession Player)> _watchers = new();
    private TimeSpan _nextUpdate;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphWatchComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphWatchComponent, XenomorphWatchActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphWatchComponent, XenoWatchBuiMsg>(OnSelected);
        SubscribeLocalEvent<XenomorphWatchComponent, MoveInputEvent>(OnMove);
        SubscribeLocalEvent<XenomorphWatchComponent, ComponentShutdown>(OnRemoved);
        SubscribeLocalEvent<XenomorphWatchComponent, PlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<XenomorphWatchComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnRecipients);
    }

    private bool HasQueen()
    {
        var queens = EntityQueryEnumerator<XenomorphRoyalComponent>();
        while (queens.MoveNext(out var uid, out _)) if (_mobs.IsAlive(uid)) return true;
        return false;
    }

    private void OnInit(Entity<XenomorphWatchComponent> ent, ref MapInitEvent args) => _actions.AddAction(ent, "ActionXenomorphWatch");

    private void OnAction(Entity<XenomorphWatchComponent> ent, ref XenomorphWatchActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent)) return;
        args.Handled = true;
        if (ent.Comp.Watching != null) { Stop(ent); return; }
        if (!HasQueen()) { _popup.PopupEntity(Loc.GetString("xenomorph-watch-no-queen"), ent, ent); return; }
        var members = new List<Xeno>();
        var query = EntityQueryEnumerator<XenomorphWatchComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out _, out var meta))
            if (uid != ent.Owner && !_mobs.IsDead(uid)) members.Add(new Xeno(GetNetEntity(uid), Name(uid), meta.EntityPrototype?.ID));
        members.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        _ui.SetUiState(ent.Owner, XenoWatchUIKey.Key, new XenoWatchBuiState(members));
        _ui.TryOpenUi(ent.Owner, XenoWatchUIKey.Key, ent.Owner);
    }

    private void OnSelected(Entity<XenomorphWatchComponent> ent, ref XenoWatchBuiMsg args)
    {
        if (args.Actor != ent.Owner || !Equals(args.UiKey, XenoWatchUIKey.Key)) return;
        Watch(ent, GetEntity(args.Target));
        _ui.CloseUi(ent.Owner, XenoWatchUIKey.Key, ent.Owner);
    }

    public bool Watch(Entity<XenomorphWatchComponent> watcher, EntityUid target)
    {
        if (watcher.Owner == target || !HasQueen() || !_mobs.IsAlive(watcher) || TerminatingOrDeleted(target)
            || !HasComp<XenomorphWatchComponent>(target) || _mobs.IsDead(target) || !TryComp<ActorComponent>(watcher, out var actor)) return false;
        Stop(watcher);
        watcher.Comp.Watching = target;
        if (TryComp<XenomorphRoyalComponent>(watcher, out var royal)) royal.Watched = target;
        _watchers[watcher] = (target, actor.PlayerSession);
        _views.AddViewSubscriber(target, actor.PlayerSession);
        _eye.SetTarget(watcher, target);
        Dirty(watcher);
        foreach (var action in _actions.GetActions(watcher))
            if (_actions.GetEvent(action) is XenomorphWatchActionEvent) _actions.SetToggled(action.Owner, true);
        return true;
    }

    public void Stop(Entity<XenomorphWatchComponent> ent)
    {
        if (_watchers.Remove(ent, out var old) && !TerminatingOrDeleted(old.Target)) _views.RemoveViewSubscriber(old.Target, old.Player);
        ent.Comp.Watching = null;
        if (TryComp<XenomorphRoyalComponent>(ent, out var royal)) royal.Watched = null;
        if (TerminatingOrDeleted(ent)) return;
        _eye.SetTarget(ent, null);
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphWatchActionEvent) _actions.SetToggled(action.Owner, false);
    }

    private void OnMove(Entity<XenomorphWatchComponent> ent, ref MoveInputEvent args) { if (args.HasDirectionalMovement && ent.Comp.Watching != null) Stop(ent); }
    private void OnRemoved(Entity<XenomorphWatchComponent> ent, ref ComponentShutdown args) => Stop(ent);
    private void OnDetached(Entity<XenomorphWatchComponent> ent, ref PlayerDetachedEvent args) => Stop(ent);
    private void OnState(Entity<XenomorphWatchComponent> ent, ref MobStateChangedEvent args) { if (args.NewMobState != MobState.Alive) Stop(ent); }

    private void OnRecipients(ExpandICChatRecipientsEvent args)
    {
        var source = Transform(args.Source).Coordinates;
        foreach (var (_, watch) in _watchers)
            if (!TerminatingOrDeleted(watch.Target) && source.TryDistance(EntityManager, Transform(watch.Target).Coordinates, out var distance) && distance <= args.VoiceRange)
                args.Recipients.TryAdd(watch.Player, new ChatSystem.ICChatRecipientData(distance, false, true));
    }

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextUpdate) return;
        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(.5);
        var queen = HasQueen();
        foreach (var (uid, watch) in _watchers.ToArray())
            if (!queen || TerminatingOrDeleted(watch.Target) || _mobs.IsDead(watch.Target) || !HasComp<XenomorphWatchComponent>(watch.Target))
                if (TryComp<XenomorphWatchComponent>(uid, out var comp)) Stop((uid, comp));
    }
}
