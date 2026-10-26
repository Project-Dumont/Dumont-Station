using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphConstructionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedXenomorphDesignSystem _design = default!;
    [Dependency] private XenomorphDesignerSystem _visual = default!;
    [Dependency] private TagSystem _tags = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphConstructionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<XenomorphConstructionComponent, XenomorphChooseResinActionEvent>(OnChoose);
        SubscribeLocalEvent<XenomorphConstructionComponent, RadialSelectorSelectedMessage>(OnSelect);
        SubscribeLocalEvent<XenomorphConstructionComponent, XenomorphSecreteResinActionEvent>(OnSecrete);
        SubscribeLocalEvent<XenomorphConstructionComponent, XenomorphConstructionDoAfterEvent>(OnBuilt);
    }

    private void OnStartup(Entity<XenomorphConstructionComponent> ent, ref ComponentStartup args)
    {
        var choices = Choices(ent);
        if (!choices.Contains(ent.Comp.Choice))
        {
            ent.Comp.Choice = choices[0];
            Dirty(ent);
        }
    }

    public List<EntProtoId> Choices(EntityUid user)
    {
        var list = new List<EntProtoId> { "WallResin", "ResinMembrane", "ResinDoor", "DumontXenomorphDesignResidueThick", "DumontXenomorphFastResin", "DumontXenomorphWallNest" };
        var caste = Comp<XenomorphComponent>(user).Caste.Id;
        if (caste != "Reaper") list.Add("DumontXenomorphResinSpikes");
        if (caste == "Hivelord" && !HasComp<XenomorphResinWhispererComponent>(user))
        {
            list[0] = "DumontXenomorphResinWallThick";
            list[1] = "DumontXenomorphResinMembraneThick";
            list[2] = "DumontXenomorphResinDoorThick";
        }
        if (caste is "Queen" or "Hivelord" && !HasComp<XenomorphResinWhispererComponent>(user))
        {
            list.Add("DumontXenomorphReflectiveResinWall");
            list.Add("DumontHiveAcidPillar");
        }
        if (HasComp<XenomorphResinWhispererComponent>(user)) list.Add("ResinWeedNode");
        return list;
    }

    private bool Ready(EntityUid user) => _mobs.IsAlive(user) && _blocker.CanConsciouslyPerformAction(user)
        && (!TryComp<XenomorphRestComponent>(user, out var rest) || !rest.Resting);

    private void OnChoose(Entity<XenomorphConstructionComponent> ent, ref XenomorphChooseResinActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        var entries = Choices(ent).Select(choice => new RadialSelectorEntry { Prototype = choice.Id }).ToList();
        _ui.SetUiState(ent.Owner, XenomorphConstructionUiKey.Key, new RadialSelectorState(entries));
        _ui.TryToggleUi(ent.Owner, XenomorphConstructionUiKey.Key, ent.Owner);
        args.Handled = true;
    }

    private void OnSelect(Entity<XenomorphConstructionComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        var selected = args.SelectedItem;
        if (!Equals(args.UiKey, XenomorphConstructionUiKey.Key) || !Ready(ent)
            || !Choices(ent).Any(c => c.Id == selected)) return;
        ent.Comp.Choice = args.SelectedItem;
        Dirty(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphSecreteResinActionEvent)
                _actions.SetIcon(action.Owner, new SpriteSpecifier.EntityPrototype(ent.Comp.Choice));
        _ui.CloseUi(ent.Owner, XenomorphConstructionUiKey.Key, args.Actor);
    }

    private EntityCoordinates Snap(EntityCoordinates coordinates)
    {
        if (_transform.GetGrid(coordinates) is not { } grid) return coordinates;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + .5f, MathF.Floor(local.Y) + .5f);
    }

    private int BaseCost(string choice) => choice switch
    {
        "ResinDoor" or "DumontXenomorphResinWallThick" or "DumontXenomorphResinMembraneThick" or "ResinWeedNode" => 75,
        "DumontXenomorphResinDoorThick" => 100,
        "DumontXenomorphDesignResidueThick" => 55,
        "DumontXenomorphFastResin" => 35,
        "DumontXenomorphResinSpikes" => 80,
        "DumontXenomorphReflectiveResinWall" => 145,
        "DumontHiveAcidPillar" => 250,
        _ => 50,
    };

    private int Cost(EntityUid user, EntityCoordinates coordinates, string choice)
    {
        _design.Modifiers(user, coordinates, out _, out var multiplier);
        return (int)MathF.Ceiling(BaseCost(choice) * multiplier);
    }

    private bool CanBuild(EntityUid user, EntityCoordinates coordinates, EntProtoId choice)
    {
        if (!Ready(user) || !Choices(user).Contains(choice) || Transform(user).GridUid is not { } grid
            || _transform.GetGrid(coordinates) != grid) return false;
        if (TryComp<XenomorphConstructionComponent>(user, out var builder) && builder.Built.TryGetValue(choice.Id, out var built))
        {
            built.RemoveAll(uid => !Exists(uid) || TerminatingOrDeleted(uid));
            var limit = choice.Id == "DumontXenomorphReflectiveResinWall" ? 5 : choice.Id == "DumontXenomorphResinSpikes" ? 15 : int.MaxValue;
            if (built.Count >= limit) return false;
        }
        var near = _interaction.InRangeUnobstructed(user, coordinates, 2);
        if (!near && (!TryComp<XenomorphResinWhispererComponent>(user, out var remote)
            || !Comp<XenomorphComponent>(user).OnWeed || !_interaction.InRangeUnobstructed(user, coordinates, remote.Range))) return false;
        var tile = _turf.GetTileRef(coordinates);
        if (tile is not { } turf || turf.Tile.IsEmpty || _turf.IsTileBlocked(turf, CollisionGroup.Impassable | CollisionGroup.HighImpassable)) return false;
        var weed = false;
        foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates, LookupFlags.All))
        {
            if (HasComp<XenomorphHiveStructureComponent>(uid) || HasComp<XenomorphAcidPillarComponent>(uid)) return false;
            if (HasComp<XenomorphWeedWallSpreadComponent>(uid)) weed = true;
            if (HasComp<XenomorphStickyResinComponent>(uid) || HasComp<XenomorphResinFloorComponent>(uid)) return false;
        }
        return (weed || choice.Id == "ResinWeedNode") && _plasma.HasPlasma(user, Cost(user, coordinates, choice.Id));
    }

    private void Busy(EntityUid user, bool busy)
    {
        foreach (var action in _actions.GetActions(user))
            if (_actions.GetEvent(action) is XenomorphSecreteResinActionEvent) _actions.SetToggled(action.Owner, busy);
    }

    private void OnSecrete(Entity<XenomorphConstructionComponent> ent, ref XenomorphSecreteResinActionEvent args)
    {
        if (args.Handled) return;
        if (ent.Comp.Choice.Id == "DumontXenomorphWallNest")
        {
            var target = args.Entity;
            if (target == null)
                foreach (var uid in _lookup.GetEntitiesIntersecting(Snap(args.Target), LookupFlags.All))
                    if (_tags.HasTag(uid, "Wall")) { target = uid; break; }
            if (target is not { } wall) return;
            var nest = new XenomorphWallNestActionEvent { Target = wall, Performer = ent, Action = args.Action };
            RaiseLocalEvent(ent, nest);
            args.Handled = nest.Handled;
            return;
        }
        var coordinates = Snap(args.Target);
        if (!CanBuild(ent, coordinates, ent.Comp.Choice)) return;
        var remote = !_interaction.InRangeUnobstructed(ent.Owner, coordinates, 2);
        var delay = Comp<XenomorphComponent>(ent).Caste == "Hivelord" ? 1f : 2f;
        if (remote && TryComp<XenomorphResinWhispererComponent>(ent, out var whisperer)) delay *= whisperer.RemoteDelayMultiplier;
        _design.Modifiers(ent, coordinates, out var speed, out _);
        delay *= speed;
        var floor = ent.Comp.Choice.Id is "DumontXenomorphFastResin" or "DumontXenomorphDesignResidueThick";
        if (floor) delay *= .5f;
        if (ent.Comp.Choice.Id == "DumontHiveAcidPillar") delay *= 6;
        EntityUid? effect = null;
        if (!floor && ent.Comp.Choice.Id != "DumontXenomorphResinSpikes" && ent.Comp.Choice.Id != "ResinWeedNode" && ent.Comp.Choice.Id != "DumontHiveAcidPillar")
            effect = _visual.CreateBuildEffect(coordinates, ent.Comp.Choice.Id.Contains("Door"), ent.Comp.Choice.Id.Contains("Thick"), delay, ent.Comp.Choice.Id.Contains("Membrane"));
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, delay,
            new XenomorphConstructionDoAfterEvent { Choice = ent.Comp.Choice, Coordinates = GetNetCoordinates(coordinates), Effect = GetNetEntity(effect) }, ent)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true, BlockDuplicate = true, CancelDuplicate = false });
        if (args.Handled)
        {
            Busy(ent, true);
            _visual.PlayBuildSound(coordinates);
        }
        else if (Exists(effect)) QueueDel(effect);
    }

    private void OnBuilt(Entity<XenomorphConstructionComponent> ent, ref XenomorphConstructionDoAfterEvent args)
    {
        if (args.Effect is { } net && TryGetEntity(net, out var effect) && Exists(effect)) QueueDel(effect.Value);
        Busy(ent, false);
        var coordinates = GetCoordinates(args.Coordinates);
        if (args.Cancelled || args.Handled || !CanBuild(ent, coordinates, args.Choice)
            || !_plasma.ChangePlasmaAmount(ent, -Cost(ent, coordinates, args.Choice.Id))) return;
        var structure = Spawn(args.Choice, coordinates);
        if (!ent.Comp.Built.TryGetValue(args.Choice.Id, out var built))
            ent.Comp.Built[args.Choice.Id] = built = new List<EntityUid>();
        built.Add(structure);
        args.Handled = true;
    }
}
