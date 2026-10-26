using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Mobs.Systems;
using Content.Server._Dumont.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Xenomorphs.Queen;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.DoAfter;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._White.Xenomorphs.Evolution;

public sealed partial class XenomorphEvolutionSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private IComponentFactory _componentFactory = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _protoManager = default!;

    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private JitteringSystem _jitter = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    [Dependency] private XenomorphHiveRanksSystem _ranks = default!;
    [Dependency] private XenomorphLineageSystem _lineage = default!;

    [Dependency] private XenomorphQueenSystem _queen = default!; // Dumont

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphEvolutionComponent, MapInitEvent>(OnXenomorphEvolutionMapInit);
        SubscribeLocalEvent<XenomorphEvolutionComponent, ComponentShutdown>(OnXenomorphEvolutionShutdown);
        SubscribeLocalEvent<XenomorphEvolutionComponent, EvolutionsActionEvent>(OnEvolutionsAction);
        SubscribeLocalEvent<XenomorphEvolutionComponent, RadialSelectorSelectedMessage>(OnEvolutionRecieved);
        SubscribeLocalEvent<XenomorphEvolutionComponent, XenomorphEvolutionDoAfterEvent>(OnXenomorphEvolutionDoAfter);
    }

    private void OnXenomorphEvolutionMapInit(EntityUid uid, XenomorphEvolutionComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.EvolutionAction, component.EvolutionActionId);

    private void OnXenomorphEvolutionShutdown(EntityUid uid, XenomorphEvolutionComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.EvolutionAction);

    private void OnEvolutionsAction(EntityUid uid, XenomorphEvolutionComponent component, ref EvolutionsActionEvent args)
    {
        if (args.Handled)
            return;

        var choices = _lineage.GetEvolutions(uid, component);
        if (choices.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-final-caste"), uid, uid);
            return;
        }

        if (choices.Count == 1)
        {
            if (component.Points < component.Max)
            {
                _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-not-enough-points", ("seconds", (component.Max - component.Points) / component.PointsPerSecond)), uid, uid);
                return;
            }

            args.Handled = Evolve(uid, choices[0].Prototype, component.EvolutionDelay);
            return;
        }

        _ui.TryToggleUi(uid, RadialSelectorUiKey.Key, uid);
        _ui.SetUiState(uid, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(choices));

        args.Handled = true;
    }

    private void OnEvolutionRecieved(EntityUid uid, XenomorphEvolutionComponent component, RadialSelectorSelectedMessage args)
    {
        if (Equals(args.UiKey, XenomorphConstructionUiKey.Key) || Equals(args.UiKey, XenomorphHiveUiKey.Construction)) return;
        if (component.Points < component.Max)
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-not-enough-points", ("seconds", (component.Max - component.Points) / component.PointsPerSecond)), uid, uid);
            return;
        }

        if (!_lineage.GetEvolutions(uid, component).Any(entry => entry.Prototype == args.SelectedItem)) // Dumont
            return;

        if (!Evolve(uid, args.SelectedItem, component.EvolutionDelay))
            return;

        var actor = args.Actor;
        _ui.CloseUi(uid, RadialSelectorUiKey.Key, actor);
    }

    private void OnXenomorphEvolutionDoAfter(EntityUid uid, XenomorphEvolutionComponent component, ref XenomorphEvolutionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !CanStartEvolution(uid) || !_lineage.CanEvolve(uid, args.Choice)
            || !_mind.TryGetMind(uid, out var mindUid, out var mind))
            return;

        if (!_ranks.CanEvolve(uid, args.Caste)) return;

        if (args.Caste == "Queen" && _queen.IsQueenAlive(HasComp<XenomorphLineageComponent>(uid))) // Dumont
            return;

        var ev = new BeforeXenomorphEvolutionEvent(args.Caste, args.CheckNeedCasteDeath);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return;

        args.Handled = true;

        var coordinates = _transform.GetMoverCoordinates(uid);
        var newXeno = Spawn(args.Choice, coordinates);

        _lineage.Transfer(uid, newXeno);
        _mind.TransferTo(mindUid, newXeno, mind:mind);
        _mind.UnVisit(mindUid, mind);
        mind.OriginalOwnedEntity = GetNetEntity(newXeno);
        mind.CharacterName = Name(newXeno);
        Dirty(mindUid, mind);

        var dropHandItemsEvent = new DropHandItemsEvent();
        RaiseLocalEvent(uid, ref dropHandItemsEvent);
        RaiseLocalEvent(uid, new AfterXenomorphEvolutionEvent(newXeno, mindUid, args.Caste));

        _adminLog.Add(LogType.Mind, $"{ToPrettyString(uid)} evolved into {ToPrettyString(newXeno)}");

        Del(uid);

        _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-end"), newXeno, newXeno);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<XenomorphEvolutionComponent>();
        while (query.MoveNext(out var uid, out var alienEvolution))
        {
            if (alienEvolution.Points >= alienEvolution.Max || time < alienEvolution.NextPointsAt || _container.IsEntityInContainer(uid))
                continue;

            alienEvolution.NextPointsAt = time + TimeSpan.FromSeconds(1);
            alienEvolution.Points = FixedPoint2.Min(alienEvolution.Max, alienEvolution.Points + alienEvolution.PointsPerSecond); // Dumont

            if (alienEvolution.Points != alienEvolution.Max)
                continue;

            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-ready"), uid, uid, PopupType.Large);
        }
    }

    private bool CanStartEvolution(EntityUid uid)
    {
        if (!HasComp<XenomorphLineageComponent>(uid))
            return true;

        return _mobState.IsAlive(uid)
            && TryComp<XenomorphEvolutionComponent>(uid, out var evolution)
            && evolution.Points >= evolution.Max
            && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting)
            && (!TryComp<XenomorphFortifyComponent>(uid, out var fortify) || !fortify.Fortified)
            && (!TryComp<XenomorphOvipositorComponent>(uid, out var ovipositor) || !ovipositor.Attached);
    }
    public bool Evolve(EntityUid uid, string? evolveTo, TimeSpan evolutionDelay, bool checkNeedCasteDeath = true)
    {
        if (!CanStartEvolution(uid) || evolveTo == null
            || !_lineage.CanEvolve(uid, evolveTo)
            || !_protoManager.TryIndex(evolveTo, out var xenomorphPrototype)
            || !xenomorphPrototype.TryGetComponent<XenomorphComponent>(out var xenomorph, _componentFactory)) // Goobstation
            return false;

        // Dumont start
        if (!_ranks.CanEvolve(uid, xenomorph.Caste)) return false;
        if (xenomorph.Caste == "Queen" && _queen.IsQueenAlive(HasComp<XenomorphLineageComponent>(uid)))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-no-cast-slot", ("caste", Loc.GetString("xenomorph-caste-queen"))), uid, uid);
            return false;
        }
        // Dumont end

        var ev = new BeforeXenomorphEvolutionEvent(xenomorph.Caste, checkNeedCasteDeath);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        var doAfterEvent = new XenomorphEvolutionDoAfterEvent(evolveTo, xenomorph.Caste, checkNeedCasteDeath);
        var doAfter = new DoAfterArgs(EntityManager, uid, evolutionDelay, doAfterEvent, uid);

        if (!_doAfter.TryStartDoAfter(doAfter))
            return false;

        _jitter.DoJitter(uid, evolutionDelay, true, 80, 8, true);

        var popupOthers = Loc.GetString("xenomorphs-evolution-start-others", ("uid", uid));
        _popup.PopupEntity(popupOthers, uid, Filter.PvsExcept(uid), true, PopupType.Medium);

        var popupSelf = Loc.GetString("xenomorphs-evolution-start-self");
        _popup.PopupEntity(popupSelf, uid, uid, PopupType.Medium);

        return true;
    }
}
