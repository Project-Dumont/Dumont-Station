using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Ghost.Roles;
using Content.Server.Mind;
using Content.Server.Ghost.Roles.Components;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphCarrierChildRoleComponent : Component
{
    public EntityUid Carrier;
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphCarrierRecoveryComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan RecoverAt;
}

public sealed partial class XenomorphCarrierSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private ThrowingSystem _throw = default!;
    [Dependency] private GhostRoleSystem _ghost = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private XenomorphCarrierConstructionSystem _construction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphCarrierComponent, XenomorphCarrierParasiteActionEvent>(OnParasite);
        SubscribeLocalEvent<XenomorphCarrierComponent, XenomorphCarrierEggActionEvent>(OnEgg);
        SubscribeLocalEvent<XenomorphCarrierComponent, XenomorphCarrierReserveActionEvent>(OnReserve);
        SubscribeLocalEvent<XenomorphCarrierComponent, XenomorphCarrierReserveMessage>(OnReserveMessage);
        SubscribeLocalEvent<XenomorphCarrierComponent, XenomorphGenerateEggsActionEvent>(OnGenerate);
        SubscribeLocalEvent<XenomorphCarrierComponent, UserActivateInWorldEvent>(OnUse);
        SubscribeLocalEvent<XenomorphCarrierComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<XenomorphCarrierComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphCarrierComponent, AfterXenomorphEvolutionEvent>(OnEvolution);
        SubscribeLocalEvent<XenomorphCarrierComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphCarrierChildRoleComponent, TakeGhostRoleEvent>(OnTakeRole);
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanConsciouslyPerformAction(uid)
        && !_containers.IsEntityInContainer(uid);

    private bool CanStoreParasite(EntityUid uid) => HasComp<FaceHuggerComponent>(uid) && !_mobs.IsDead(uid)
        && (!TryComp<MindContainerComponent>(uid, out var mind) || !mind.HasMind)
        && (!TryComp<ClothingComponent>(uid, out var clothing) || clothing.InSlot == null);

    private void Report(Entity<XenomorphCarrierComponent> ent) =>
        _popup.PopupEntity(Loc.GetString("xenomorph-carrier-storage",
            ("parasites", ent.Comp.Parasites), ("maxParasites", ent.Comp.MaxParasites),
            ("eggs", ent.Comp.Eggs), ("maxEggs", ent.Comp.MaxEggs)), ent, ent);

    private bool Store(Entity<XenomorphCarrierComponent> ent, EntityUid item)
    {
        var parasite = CanStoreParasite(item);
        var egg = HasComp<XenomorphPortableEggComponent>(item);
        if (!parasite && !egg || TerminatingOrDeleted(item) || EntityManager.IsQueuedForDeletion(item))
            return false;
        if (parasite && ent.Comp.Parasites >= ent.Comp.MaxParasites || egg && ent.Comp.Eggs >= ent.Comp.MaxEggs)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-carrier-full"), ent, ent);
            return false;
        }
        if (parasite)
            ent.Comp.Parasites++;
        else
            ent.Comp.Eggs++;
        RemComp<GhostRoleComponent>(item);
        QueueDel(item);
        Dirty(ent);
        return true;
    }

    private void OnUse(Entity<XenomorphCarrierComponent> ent, ref UserActivateInWorldEvent args)
    {
        if (args.Handled || !Ready(ent) || _hands.GetActiveItem((ent, null)) != args.Target)
            return;
        if (Store(ent, args.Target))
        {
            args.Handled = true;
            Report(ent);
        }
    }

    private bool Collect(Entity<XenomorphCarrierComponent> ent, EntityCoordinates target, bool parasites)
    {
        if (!_interaction.InRangeUnobstructed(ent.Owner, target))
            return false;
        var found = false;
        foreach (var item in _lookup.GetEntitiesIntersecting(target))
        {
            if (TerminatingOrDeleted(item) || EntityManager.IsQueuedForDeletion(item) || _containers.IsEntityInContainer(item)
                || (parasites ? !CanStoreParasite(item) : !HasComp<XenomorphPortableEggComponent>(item)))
                continue;
            found = true;
            Store(ent, item);
        }
        if (found)
            Report(ent);
        return found;
    }

    private void OnParasite(Entity<XenomorphCarrierComponent> ent, ref XenomorphCarrierParasiteActionEvent args)
    {
        if (args.Handled || !Ready(ent) || ent.Comp.MaxParasites <= 0 || !args.Target.IsValid(EntityManager))
            return;
        args.Handled = true;
        if (Collect(ent, args.Target, true))
            return;
        if (_hands.GetActiveItem((ent, null)) is { } held && HasComp<FaceHuggerComponent>(held))
        {
            var target = _transform.ToMapCoordinates(args.Target);
            var origin = _transform.GetMapCoordinates(ent);
            if (origin.MapId != target.MapId || !_hands.TryDrop((ent, null), held))
                return;
            var direction = target.Position - origin.Position;
            if (!float.IsFinite(direction.LengthSquared()))
                return;
            if (direction.LengthSquared() > 16)
                direction = Vector2.Normalize(direction) * 4;
            _throw.TryThrow(held, direction, user: ent, doSpin: false);
            SuspendParasite(held, 5);
            _actions.SetCooldown(args.Action.Owner, TimeSpan.FromSeconds(2));
            return;
        }
        if (ent.Comp.Parasites == 0 || !_hands.TryGetEmptyHand((ent, null), out _))
        {
            Report(ent);
            return;
        }
        if (TryComp<FlammableComponent>(ent, out var fire) && fire.OnFire)
            return;
        var parasite = Spawn("MobXenomorphFaceHugger", _transform.GetMoverCoordinates(ent));
        SuspendParasite(parasite, 1);
        if (!_hands.TryPickupAnyHand(ent, parasite))
        {
            QueueDel(parasite);
            return;
        }
        ent.Comp.Parasites--;
        Dirty(ent);
        Report(ent);
    }

    private void SuspendParasite(EntityUid uid, double seconds)
    {
        if (!TryComp<FaceHuggerComponent>(uid, out var facehugger)
            || TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
            return;
        EnsureComp<XenomorphCarrierRecoveryComponent>(uid).RecoverAt = _timing.CurTime + TimeSpan.FromSeconds(seconds);
        facehugger.Active = false;
        facehugger.RestIn = _timing.CurTime + TimeSpan.FromSeconds(seconds);
    }

    private void OnEgg(Entity<XenomorphCarrierComponent> ent, ref XenomorphCarrierEggActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !args.Target.IsValid(EntityManager))
            return;
        args.Handled = true;
        if (Collect(ent, args.Target, false))
            return;
        if (ent.Comp.Eggs == 0 || !_hands.TryGetEmptyHand((ent, null), out _))
        {
            Report(ent);
            return;
        }
        var egg = Spawn("DumontXenomorphEggItem", _transform.GetMoverCoordinates(ent));
        if (!_hands.TryPickupAnyHand(ent, egg))
        {
            QueueDel(egg);
            return;
        }
        ent.Comp.Eggs--;
        Dirty(ent);
        Report(ent);
    }

    private void OnReserve(Entity<XenomorphCarrierComponent> ent, ref XenomorphCarrierReserveActionEvent args)
    {
        if (args.Handled || !Ready(ent) || ent.Comp.MaxParasites <= 0)
            return;
        args.Handled = _ui.TryOpenUi(ent.Owner, XenomorphCarrierUiKey.Reserve, ent.Owner);
    }

    private void OnReserveMessage(Entity<XenomorphCarrierComponent> ent, ref XenomorphCarrierReserveMessage args)
    {
        if (args.Actor != ent.Owner || !Ready(ent))
            return;
        ent.Comp.ReservedParasites = Math.Clamp(args.Count, 0, ent.Comp.MaxParasites);
        Dirty(ent);
    }

    private void OnGenerate(Entity<XenomorphCarrierComponent> ent, ref XenomorphGenerateEggsActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !ent.Comp.CanGenerateEggs)
            return;
        if (!ent.Comp.GeneratingEggs && !_plasma.HasPlasma(ent, 15))
            return;
        SetGenerating(ent, !ent.Comp.GeneratingEggs);
        args.Handled = true;
    }

    private void SetGenerating(Entity<XenomorphCarrierComponent> ent, bool active)
    {
        ent.Comp.GeneratingEggs = active;
        ent.Comp.NextEgg = active ? _timing.CurTime + TimeSpan.FromSeconds(30) : null;
        ent.Comp.NextDrain = active ? _timing.CurTime + TimeSpan.FromSeconds(2) : null;
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphGenerateEggsActionEvent)
                _actions.SetToggled(action.Owner, active);
        Dirty(ent);
    }

    private void OnExamine(Entity<XenomorphCarrierComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<XenomorphComponent>(args.Examiner) || HasComp<GhostComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("xenomorph-carrier-storage",
                ("parasites", ent.Comp.Parasites), ("maxParasites", ent.Comp.MaxParasites),
                ("eggs", ent.Comp.Eggs), ("maxEggs", ent.Comp.MaxEggs)));
    }

    private void OnEvolution(Entity<XenomorphCarrierComponent> ent, ref AfterXenomorphEvolutionEvent args) => Release(ent, 1);

    private void OnMobState(Entity<XenomorphCarrierComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;
        SetGenerating(ent, false);
        _construction.DecaySustainedEggs(ent);
        Release(ent, 0.75f);
    }

    private void OnShutdown(Entity<XenomorphCarrierComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ChildRole is { } role && Exists(role))
            QueueDel(role);
    }

    private void Release(Entity<XenomorphCarrierComponent> ent, float survival)
    {
        for (var i = 0; i < ent.Comp.Parasites + ent.Comp.Eggs; i++)
        {
            if (!_random.Prob(survival))
                continue;
            var item = Spawn(i < ent.Comp.Parasites ? "MobXenomorphFaceHugger" : "DumontXenomorphEggItem",
                _transform.GetMoverCoordinates(ent));
            SuspendParasite(item, 5);
            _throw.TryThrow(item, _random.NextVector2(0.7f), 3, doSpin: false);
        }
        ent.Comp.Parasites = ent.Comp.Eggs = 0;
        Dirty(ent);
    }

    private void OnTakeRole(Entity<XenomorphCarrierChildRoleComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (!TryComp<XenomorphCarrierComponent>(ent.Comp.Carrier, out var carrier) || !Ready(ent.Comp.Carrier)
            || carrier.Parasites <= carrier.ReservedParasites)
            return;
        carrier.Parasites--;
        Dirty(ent.Comp.Carrier, carrier);
        var parasite = Spawn("MobXenomorphFaceHugger", _transform.GetMoverCoordinates(ent.Comp.Carrier));
        _mind.MakeSentient(parasite, true, true);
        _ghost.GhostRoleInternalCreateMindAndTransfer(args.Player, ent, parasite);
        args.TookRole = true;
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var recovering = EntityQueryEnumerator<XenomorphCarrierRecoveryComponent>();
        while (recovering.MoveNext(out var child, out var recovery))
            if (now >= recovery.RecoverAt)
                RemCompDeferred<XenomorphCarrierRecoveryComponent>(child);
        var query = EntityQueryEnumerator<XenomorphCarrierComponent>();
        while (query.MoveNext(out var uid, out var carrier))
        {
            if (Ready(uid) && carrier.Parasites > carrier.ReservedParasites)
            {
                if (carrier.ChildRole == null || !Exists(carrier.ChildRole.Value))
                {
                    var role = Spawn("DumontXenomorphCarrierChildRole", new EntityCoordinates(uid, Vector2.Zero));
                    Comp<XenomorphCarrierChildRoleComponent>(role).Carrier = uid;
                    carrier.ChildRole = role;
                }
            }
            else if (carrier.ChildRole is { } role && Exists(role))
            {
                QueueDel(role);
                carrier.ChildRole = null;
            }
            if (!carrier.GeneratingEggs)
                continue;
            if (carrier.Eggs >= carrier.MaxEggs)
            {
                carrier.NextEgg = carrier.NextDrain = null;
                continue;
            }
            if (carrier.NextEgg == null)
            {
                carrier.NextEgg = now + TimeSpan.FromSeconds(30);
                carrier.NextDrain = now + TimeSpan.FromSeconds(2);
            }
            if (now >= carrier.NextDrain)
            {
                if (!_plasma.HasPlasma(uid, 15))
                {
                    SetGenerating((uid, carrier), false);
                    continue;
                }
                _plasma.ChangePlasmaAmount(uid, -15);
                carrier.NextDrain = now + TimeSpan.FromSeconds(2);
            }
            if (now < carrier.NextEgg)
                continue;
            carrier.Eggs++;
            carrier.NextEgg = now + TimeSpan.FromSeconds(30);
            Dirty(uid, carrier);
            Report((uid, carrier));
        }
    }
}
