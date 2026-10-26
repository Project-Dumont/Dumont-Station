using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveConstructionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private XenomorphDesignerSystem _visual = default!;

    private static readonly EntProtoId[] Choices = { "DumontHiveCoreConstructionNode", "DumontHiveRecoveryConstructionNode", "DumontHivePlasmaConstructionNode", "DumontHiveEggMorpherConstructionNode", "DumontHiveClusterConstructionNode" };

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHiveBuilderComponent, MapInitEvent>(OnBuilderInit);
        SubscribeLocalEvent<XenomorphHiveBuilderComponent, XenomorphOrderHiveConstructionActionEvent>(OnChoose);
        SubscribeLocalEvent<XenomorphHiveBuilderComponent, RadialSelectorSelectedMessage>(OnSelected);
        SubscribeLocalEvent<XenomorphHiveBuilderComponent, XenomorphPlaceHiveConstructionActionEvent>(OnPlace);
        SubscribeLocalEvent<XenomorphHiveBuilderComponent, XenomorphOrderHiveConstructionDoAfterEvent>(OnOrdered);
        SubscribeLocalEvent<XenomorphHiveConstructionNodeComponent, ActivateInWorldEvent>(OnAddPlasma);
        SubscribeLocalEvent<XenomorphHiveConstructionNodeComponent, XenomorphAddHivePlasmaDoAfterEvent>(OnPlasmaAdded);
        SubscribeLocalEvent<XenomorphHiveConstructionNodeComponent, ExaminedEvent>(OnNodeExamined);
        SubscribeLocalEvent<XenomorphHiveStructureComponent, ActivateInWorldEvent>(OnRepair);
        SubscribeLocalEvent<XenomorphHiveStructureComponent, XenomorphRepairHiveDoAfterEvent>(OnRepaired);
    }

    private bool Ready(EntityUid user) => HasComp<XenomorphLineageComponent>(user) && _mobs.IsAlive(user)
        && _blocker.CanConsciouslyPerformAction(user)
        && (!TryComp<XenomorphRestComponent>(user, out var rest) || !rest.Resting);

    private void OnBuilderInit(Entity<XenomorphHiveBuilderComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, "ActionXenomorphOrderHiveConstruction");
        _actions.AddAction(ent, "ActionXenomorphPlaceHiveConstruction");
    }

    private void OnChoose(Entity<XenomorphHiveBuilderComponent> ent, ref XenomorphOrderHiveConstructionActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        _ui.SetUiState(ent.Owner, XenomorphHiveUiKey.Construction,
            new RadialSelectorState(Choices.Select(c => new RadialSelectorEntry { Prototype = c.Id }).ToList()));
        args.Handled = _ui.TryToggleUi(ent.Owner, XenomorphHiveUiKey.Construction, ent.Owner);
    }

    private void OnSelected(Entity<XenomorphHiveBuilderComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        var selected = args.SelectedItem;
        if (!Equals(args.UiKey, XenomorphHiveUiKey.Construction) || args.Actor != ent.Owner || !Ready(ent)
            || !Choices.Any(c => c.Id == selected)) return;
        ent.Comp.Choice = args.SelectedItem;
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphPlaceHiveConstructionActionEvent)
                _actions.SetIcon(action.Owner, new Robust.Shared.Utility.SpriteSpecifier.EntityPrototype(ent.Comp.Choice));
        _ui.CloseUi(ent.Owner, XenomorphHiveUiKey.Construction, ent.Owner);
    }

    public bool HasCore()
    {
        var query = EntityQueryEnumerator<XenomorphHiveCoreComponent>();
        while (query.MoveNext(out var uid, out _))
            if (!TerminatingOrDeleted(uid)) return true;
        return false;
    }

    private EntityCoordinates Snap(EntityCoordinates target)
    {
        if (_transform.GetGrid(target) is not { } grid) return target;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(target));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + .5f, MathF.Floor(local.Y) + .5f);
    }

    private bool Failure(EntityUid user, string reason)
    {
        _popup.PopupEntity(Loc.GetString(reason), user, user);
        return false;
    }

    private bool CanOrder(EntityUid user, EntityCoordinates target, EntProtoId choice)
    {
        if (!Ready(user) || !TryComp<XenomorphHiveBuilderComponent>(user, out var builder) || !Choices.Contains(choice)
            || _transform.GetGrid(target) is not { } grid || grid != Transform(user).GridUid
            || !_interaction.InRangeUnobstructed(user, target, builder.Range)) return false;
        var prototype = _prototypes.Index(choice);
        if (!prototype.TryGetComponent<XenomorphHiveStructureComponent>(out var structure, _factory)
            || !prototype.TryGetComponent<XenomorphHiveConstructionNodeComponent>(out var node, _factory)) return false;
        if (!_plasma.HasPlasma(user, node.InitialPlasmaCost)) return Failure(user, "xenomorph-hive-not-enough-plasma");
        if (structure.RequiresCore && !HasCore()) return Failure(user, "xenomorph-hive-requires-core");
        var count = 0;
        var structures = EntityQueryEnumerator<XenomorphHiveStructureComponent>();
        while (structures.MoveNext(out var uid, out var existing))
            if (!TerminatingOrDeleted(uid) && existing.Kind == structure.Kind) count++;
        if (count >= structure.Limit) return Failure(user, "xenomorph-hive-limit");
        var tile = _turf.GetTileRef(target);
        if (tile == null || tile.Value.Tile.IsEmpty || _turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable | CollisionGroup.HighImpassable))
            return Failure(user, "xenomorph-hive-no-space");
        if (structure.RequiresHiveWeeds && !_lookup.GetEntitiesInRange<XenomorphHiveWeedComponent>(target, .4f, LookupFlags.All).Any())
            return Failure(user, "xenomorph-hive-requires-weeds");
        foreach (var nearby in _lookup.GetEntitiesInRange<XenomorphHiveStructureComponent>(target, 1.5f, LookupFlags.All))
            if (!TerminatingOrDeleted(nearby)) return Failure(user, "xenomorph-hive-no-space");
        if (structure.RequiresSpace)
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 1; y++)
                {
                    var adjacent = _turf.GetTileRef(target.Offset(new System.Numerics.Vector2(x, y)));
                    if (adjacent == null || adjacent.Value.Tile.IsEmpty || _turf.IsTileBlocked(adjacent.Value, CollisionGroup.Impassable | CollisionGroup.HighImpassable))
                        return Failure(user, "xenomorph-hive-no-space");
                }
        return true;
    }

    private void OnPlace(Entity<XenomorphHiveBuilderComponent> ent, ref XenomorphPlaceHiveConstructionActionEvent args)
    {
        var target = Snap(args.Target);
        if (args.Handled || !CanOrder(ent, target, ent.Comp.Choice)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.OrderDelay,
            new XenomorphOrderHiveConstructionDoAfterEvent { Choice = ent.Comp.Choice, Coordinates = GetNetCoordinates(target) }, ent)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true, BlockDuplicate = true, CancelDuplicate = false });
        if (args.Handled) _visual.PlayBuildSound(target);
    }

    private void OnOrdered(Entity<XenomorphHiveBuilderComponent> ent, ref XenomorphOrderHiveConstructionDoAfterEvent args)
    {
        var target = GetCoordinates(args.Coordinates);
        if (args.Cancelled || args.Handled || !CanOrder(ent, target, args.Choice)) return;
        var prototype = _prototypes.Index(args.Choice);
        if (!prototype.TryGetComponent<XenomorphHiveConstructionNodeComponent>(out var node, _factory)
            || !_plasma.ChangePlasmaAmount(ent, -node.InitialPlasmaCost)) return;
        Spawn(args.Choice, target);
        args.Handled = true;
    }

    private bool CanContribute(EntityUid user, EntityUid target) => Ready(user) && HasComp<XenomorphComponent>(user)
        && _interaction.InRangeUnobstructed(user, target, 1.75f);

    private void OnAddPlasma(Entity<XenomorphHiveConstructionNodeComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !CanContribute(args.User, ent) || !TryComp<PlasmaVesselComponent>(args.User, out var plasma) || plasma.Plasma < 1) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(3),
            new XenomorphAddHivePlasmaDoAfterEvent(), ent, ent)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true, BlockDuplicate = true, CancelDuplicate = false });
    }

    private void OnPlasmaAdded(Entity<XenomorphHiveConstructionNodeComponent> ent, ref XenomorphAddHivePlasmaDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !CanContribute(args.User, ent) || !TryComp<PlasmaVesselComponent>(args.User, out var plasma)) return;
        if (Comp<XenomorphHiveStructureComponent>(ent).RequiresCore && !HasCore())
        { Failure(args.User, "xenomorph-hive-requires-core"); return; }
        var amount = FixedPoint2.Min(plasma.Plasma, ent.Comp.PlasmaCost - ent.Comp.PlasmaStored);
        if (amount <= 0 || !_plasma.ChangePlasmaAmount(args.User, -amount)) return;
        ent.Comp.PlasmaStored += amount;
        args.Handled = true;
        Dirty(ent);
        if (ent.Comp.PlasmaStored < ent.Comp.PlasmaCost)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-plasma-left", ("amount", ent.Comp.PlasmaCost - ent.Comp.PlasmaStored)), ent, args.User);
            return;
        }
        Spawn(ent.Comp.Spawn, Transform(ent).Coordinates);
        QueueDel(ent);
    }

    private void OnNodeExamined(Entity<XenomorphHiveConstructionNodeComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<XenomorphComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("xenomorph-hive-plasma-left", ("amount", ent.Comp.PlasmaCost - ent.Comp.PlasmaStored)));
    }

    private void OnRepair(Entity<XenomorphHiveStructureComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !HasComp<XenomorphHiveBuilderComponent>(args.User) || ent.Comp.RepairCost <= 0 || HasComp<XenomorphHiveConstructionNodeComponent>(ent)
            || !CanContribute(args.User, ent) || !TryComp<DamageableComponent>(ent, out var damage) || damage.TotalDamage <= 0) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, 4,
            new XenomorphRepairHiveDoAfterEvent(), ent, ent)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true, BlockDuplicate = true, CancelDuplicate = false });
    }

    private void OnRepaired(Entity<XenomorphHiveStructureComponent> ent, ref XenomorphRepairHiveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !CanContribute(args.User, ent) || !TryComp<PlasmaVesselComponent>(args.User, out var plasma)
            || !TryComp<DamageableComponent>(ent, out var damage) || damage.TotalDamage <= 0) return;
        var amount = FixedPoint2.Min(plasma.Plasma, ent.Comp.RepairCost - ent.Comp.RepairStored);
        if (amount <= 0 || !_plasma.ChangePlasmaAmount(args.User, -amount)) return;
        ent.Comp.RepairStored += amount;
        args.Handled = true;
        if (ent.Comp.RepairStored < ent.Comp.RepairCost)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-plasma-left", ("amount", ent.Comp.RepairCost - ent.Comp.RepairStored)), ent, args.User);
            return;
        }
        ent.Comp.RepairStored = 0;
        _damage.SetAllDamage(ent.Owner, damage, FixedPoint2.Zero);
    }
}
