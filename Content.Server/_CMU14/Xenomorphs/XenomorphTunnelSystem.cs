using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using Content.Server.DoAfter;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Hands;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphInTunnelComponent : Component
{
    public EntityUid Tunnel;
    public bool Travelling;
    public HashSet<EntityUid> DisabledActions = new();
}

public sealed partial class XenomorphTunnelSystem : EntitySystem
{
    [Dependency] private XenomorphBurrowSystem _burrow = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private MetaDataSystem _metadata = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphDigTunnelActionEvent>(OnDig);
        SubscribeLocalEvent<XenomorphBurrowComponent, XenomorphDigTunnelDoAfterEvent>(OnDug);
        SubscribeLocalEvent<XenomorphTunnelComponent, InteractHandEvent>(OnEnter);
        SubscribeLocalEvent<XenomorphTunnelComponent, XenomorphTunnelEnterDoAfterEvent>(OnEntered);
        SubscribeLocalEvent<XenomorphTunnelComponent, XenomorphTunnelTravelMessage>(OnTravel);
        SubscribeLocalEvent<XenomorphTunnelComponent, XenomorphTunnelTravelDoAfterEvent>(OnTravelled);
        SubscribeLocalEvent<XenomorphTunnelComponent, XenomorphTunnelNameMessage>(OnName);
        SubscribeLocalEvent<XenomorphTunnelComponent, XenomorphTunnelExitMessage>(OnExit);
        SubscribeLocalEvent<XenomorphTunnelComponent, ContainerRelayMovementEntityEvent>(OnMove);
        SubscribeLocalEvent<XenomorphTunnelComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<XenomorphTunnelComponent, GetVerbsEvent<ActivationVerb>>(OnRenameVerb);
        SubscribeLocalEvent<XenomorphInTunnelComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphInTunnelComponent, DropAttemptEvent>(OnDrop);
    }

    private EntityCoordinates Snap(EntityUid user)
    {
        var coordinates = _transform.GetMoverCoordinates(user);
        var grid = Transform(user).GridUid ?? coordinates.EntityId;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
    }

    private bool CanBuild(EntityUid user, EntityCoordinates coordinates) => _mobs.IsAlive(user)
        && _blocker.CanInteract(user, null) && !_containers.IsEntityInContainer(user)
        && _burrow.ValidFloor(coordinates) && !_lookup.GetEntitiesIntersecting(coordinates).Any(HasComp<XenomorphTunnelComponent>);

    private void OnDig(Entity<XenomorphBurrowComponent> ent, ref XenomorphDigTunnelActionEvent args)
    {
        if (args.Handled || ent.Comp.Active || !CanBuild(ent, Snap(ent)) || !_plasma.HasPlasma(ent, 200))
            return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 4,
            new XenomorphDigTunnelDoAfterEvent(), ent.Owner) { BreakOnMove = true, NeedHand = false });
        if (args.Handled)
        {
            ent.Comp.BuildingTunnel = true;
            Dirty(ent);
            _blocker.UpdateCanMove(ent);
        }
    }

    private void OnDug(Entity<XenomorphBurrowComponent> ent, ref XenomorphDigTunnelDoAfterEvent args)
    {
        if (args.Handled)
            return;
        ent.Comp.BuildingTunnel = false;
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
        if (args.Cancelled || !CanBuild(ent, Snap(ent)) || !_plasma.ChangePlasmaAmount(ent, -200))
        {
            foreach (var action in _actions.GetActions(ent))
                if (_actions.GetEvent(action) is XenomorphDigTunnelActionEvent)
                    _actions.RemoveCooldown(action.Owner);
            return;
        }
        var coordinates = Snap(ent);
        var tunnel = Spawn("DumontXenomorphTunnel", coordinates);
        _metadata.SetEntityName(tunnel, Loc.GetString("xenomorph-tunnel-default-name", ("x", (int)coordinates.X), ("y", (int)coordinates.Y)));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/burrowing_b.ogg"), tunnel);
        RefreshAll();
        _ui.TryOpenUi(tunnel, XenomorphTunnelUiKey.Rename, ent);
        args.Handled = true;
    }

    private bool IsXeno(EntityUid uid) => HasComp<XenomorphComponent>(uid) || HasComp<FaceHuggerComponent>(uid);

    private Container GetContainer(EntityUid uid) => _containers.EnsureContainer<Container>(uid, XenomorphTunnelComponent.Container);

    private bool CanEnter(EntityUid user, Entity<XenomorphTunnelComponent> tunnel) => IsXeno(user) && _mobs.IsAlive(user)
        && _blocker.CanMove(user) && _blocker.CanInteract(user, tunnel.Owner) && !Transform(user).Anchored
        && !_containers.IsEntityInContainer(user) && GetContainer(tunnel).Count < tunnel.Comp.MaxMobs
        && _interaction.InRangeUnobstructed(user, tunnel.Owner, 1.5f);

    private static bool Large(string caste) => caste is "Hivelord" or "Carrier" or "Reaper" or "Crusher"
        or "Ravager" or "Boiler" or "Despoiler" or "Praetorian" or "Queen";

    private float Delay(EntityUid user, bool entering)
    {
        TryComp<XenomorphComponent>(user, out var xeno);
        if (HasComp<FaceHuggerComponent>(user) || xeno?.Caste == "Larva")
            return 1;
        if (xeno != null && Large(xeno.Caste.Id))
            return entering ? 12 : 6;
        return entering ? 4 : 2;
    }

    private void OnEnter(Entity<XenomorphTunnelComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !IsXeno(args.User))
            return;
        if (!CanEnter(args.User, ent))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-tunnel-unavailable"), ent, args.User);
            return;
        }
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User,
            TimeSpan.FromSeconds(Delay(args.User, true)), new XenomorphTunnelEnterDoAfterEvent(), ent.Owner, ent.Owner)
        { BreakOnMove = true, NeedHand = false });
    }

    private void OnEntered(Entity<XenomorphTunnelComponent> ent, ref XenomorphTunnelEnterDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !CanEnter(args.User, ent))
            return;
        _pulling.StopAllPulls(args.User);
        if (!_containers.Insert(args.User, GetContainer(ent)))
            return;
        var inside = EnsureComp<XenomorphInTunnelComponent>(args.User);
        inside.Tunnel = ent;
        foreach (var action in _actions.GetActions(args.User))
        {
            if (!action.Comp.Enabled) continue;
            inside.DisabledActions.Add(action.Owner);
            _actions.SetEnabled(action.Owner, false);
        }
        OpenDestination(ent, args.User);
        args.Handled = true;
    }

    private bool CanTravel(EntityUid user, Entity<XenomorphTunnelComponent> source, Entity<XenomorphTunnelComponent> destination)
        => source.Owner != destination.Owner && _mobs.IsAlive(user) && IsXeno(user)
           && Transform(source).MapUid == Transform(destination).MapUid
           && Transform(source).MapUid != null && !TerminatingOrDeleted(destination)
           && _containers.ContainsEntity(source, user) && GetContainer(destination).Count < destination.Comp.MaxMobs;

    private void OnTravel(Entity<XenomorphTunnelComponent> ent, ref XenomorphTunnelTravelMessage args)
    {
        var destination = GetEntity(args.Destination);
        if (!TryComp<XenomorphInTunnelComponent>(args.Actor, out var inside) || inside.Travelling
            || inside.Tunnel != ent.Owner || !TryComp<XenomorphTunnelComponent>(destination, out var tunnel)
            || !CanTravel(args.Actor, ent, (destination, tunnel)))
            return;
        inside.Travelling = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Actor,
            TimeSpan.FromSeconds(Delay(args.Actor, false)),
            new XenomorphTunnelTravelDoAfterEvent { Destination = args.Destination }, ent.Owner)
        { RequireCanInteract = false, NeedHand = false, DistanceThreshold = null, BreakOnMove = true, CancelDuplicate = false });
    }

    private void OnTravelled(Entity<XenomorphTunnelComponent> ent, ref XenomorphTunnelTravelDoAfterEvent args)
    {
        if (args.Handled || !TryComp<XenomorphInTunnelComponent>(args.User, out var inside))
            return;
        inside.Travelling = false;
        var destination = GetEntity(args.Destination);
        if (args.Cancelled || inside.Tunnel != ent.Owner || !TryComp<XenomorphTunnelComponent>(destination, out var tunnel)
            || !CanTravel(args.User, ent, (destination, tunnel)) || !_containers.Insert(args.User, GetContainer(destination)))
            return;
        _ui.CloseUi(ent.Owner, XenomorphTunnelUiKey.Destination, args.User);
        inside.Tunnel = destination;
        OpenDestination(destination, args.User);
        args.Handled = true;
    }

    private void OpenDestination(EntityUid tunnel, EntityUid user)
    {
        Refresh(tunnel);
        _ui.TryOpenUi(tunnel, XenomorphTunnelUiKey.Destination, user);
    }

    private void Refresh(EntityUid uid)
    {
        var destinations = new Dictionary<NetEntity, string>();
        var map = Transform(uid).MapUid;
        var query = EntityQueryEnumerator<XenomorphTunnelComponent, MetaDataComponent>();
        while (query.MoveNext(out var other, out _, out var meta))
            if (other != uid && !TerminatingOrDeleted(other) && Transform(other).MapUid == map)
                destinations[GetNetEntity(other)] = meta.EntityName;
        var state = new XenomorphTunnelUiState(Name(uid), destinations);
        _ui.SetUiState(uid, XenomorphTunnelUiKey.Destination, state);
        _ui.SetUiState(uid, XenomorphTunnelUiKey.Rename, state);
    }

    private void RefreshAll()
    {
        var query = EntityQueryEnumerator<XenomorphTunnelComponent>();
        while (query.MoveNext(out var uid, out _))
            if (!TerminatingOrDeleted(uid)) Refresh(uid);
    }

    private void OnName(Entity<XenomorphTunnelComponent> ent, ref XenomorphTunnelNameMessage args)
    {
        var name = args.Name.Trim();
        if (!HasComp<XenomorphBurrowComponent>(args.Actor) || !_mobs.IsAlive(args.Actor)
            || name.Length is 0 or > 48 || !_interaction.InRangeAndAccessible(args.Actor, ent.Owner, 1.5f))
            return;
        _metadata.SetEntityName(ent, name);
        RefreshAll();
        _ui.CloseUi(ent.Owner, XenomorphTunnelUiKey.Rename, args.Actor);
    }

    private void OnRenameVerb(Entity<XenomorphTunnelComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !HasComp<XenomorphBurrowComponent>(args.User))
            return;
        var user = args.User;
        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString("xenomorph-tunnel-rename"),
            Act = () => { Refresh(ent); _ui.TryOpenUi(ent.Owner, XenomorphTunnelUiKey.Rename, user); },
        });
    }

    private void Restore(EntityUid user, XenomorphInTunnelComponent inside)
    {
        foreach (var action in inside.DisabledActions)
            if (Exists(action)) _actions.SetEnabled(action, true);
        _ui.CloseUi(inside.Tunnel, XenomorphTunnelUiKey.Destination, user);
        RemCompDeferred<XenomorphInTunnelComponent>(user);
    }

    public void Exit(EntityUid user)
    {
        if (!TryComp<XenomorphInTunnelComponent>(user, out var inside)) return;
        if (_containers.TryGetContainingContainer(user, out var container) && container.ID == XenomorphTunnelComponent.Container)
        {
            var coordinates = _transform.GetMoverCoordinates(container.Owner);
            _containers.Remove(user, container, force: true, destination: coordinates);
        }
        Restore(user, inside);
    }

    private void OnExit(Entity<XenomorphTunnelComponent> ent, ref XenomorphTunnelExitMessage args)
    {
        if (_containers.ContainsEntity(ent, args.Actor)) Exit(args.Actor);
    }

    private void OnMove(Entity<XenomorphTunnelComponent> ent, ref ContainerRelayMovementEntityEvent args)
    {
        if (_containers.ContainsEntity(ent, args.Entity)) Exit(args.Entity);
    }

    private void OnDeleted(Entity<XenomorphTunnelComponent> ent, ref EntityTerminatingEvent args)
    {
        if (_containers.TryGetContainer(ent, XenomorphTunnelComponent.Container, out var container))
            foreach (var user in container.ContainedEntities.ToArray()) Exit(user);
        RefreshAll();
    }

    private void OnDrop(Entity<XenomorphInTunnelComponent> ent, ref DropAttemptEvent args) => args.Cancel();

    private void OnMobState(Entity<XenomorphInTunnelComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive) Exit(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphInTunnelComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (!_containers.TryGetContainingContainer(uid, out var container) || container.Owner != comp.Tunnel
                || container.ID != XenomorphTunnelComponent.Container)
                Restore(uid, comp);
    }
}
