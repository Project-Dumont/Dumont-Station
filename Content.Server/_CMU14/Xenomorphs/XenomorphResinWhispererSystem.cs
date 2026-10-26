using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphResinWhispererSystem : EntitySystem
{
    [Dependency] private SharedXenomorphDesignSystem _design = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private AnchorableSystem _anchorable = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedDoorSystem _doors = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinWhispererComponent, XenomorphRemoteResinActionEvent>(OnBuild);
        SubscribeLocalEvent<XenomorphResinWhispererComponent, XenomorphRemoteResinDoAfterEvent>(OnBuilt);
        SubscribeLocalEvent<XenomorphResinDoorComponent, GetVerbsEvent<AlternativeVerb>>(OnDoorVerb);
    }

    private int Cost(EntityUid user, EntityCoordinates coordinates, bool door)
    {
        _design.Modifiers(user, coordinates, out _, out var multiplier);
        return (int)MathF.Ceiling((door ? 75 : 50) * multiplier);
    }

    private bool OnWeeds(EntityUid user) => TryComp<XenomorphComponent>(user, out var xeno) && xeno.OnWeed;

    private bool Visible(EntityUid user, EntityCoordinates coordinates, float range)
    {
        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
            if (_interaction.InRangeUnobstructed(user, coordinates.Offset(new Vector2(x, y) * .499f), range: range)) return true;
        return false;
    }

    private bool CanBuild(Entity<XenomorphResinWhispererComponent> user, EntityCoordinates coordinates, bool door, out bool remote)
    {
        remote = !_interaction.InRangeUnobstructed(user.Owner, coordinates, range: 2);
        if (!_mobs.IsAlive(user) || !_blocker.CanConsciouslyPerformAction(user)
            || Transform(user).GridUid is not { } gridUid || _transform.GetGrid(coordinates) != gridUid
            || !TryComp<MapGridComponent>(gridUid, out var grid)
            || !_plasma.HasPlasma(user, Cost(user, coordinates, door))) return false;
        if (remote && (!OnWeeds(user) || !Visible(user, coordinates, user.Comp.Range)))
        {
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-remote-resin-unreachable"), user, user);
            return false;
        }
        var tile = _map.TileIndicesFor(gridUid, grid, coordinates);
        return !_map.GetTileRef(gridUid, grid, tile).Tile.IsEmpty
            && _anchorable.TileFree(grid, tile, (int)(door ? CollisionGroup.TableLayer | CollisionGroup.BulletImpassable : CollisionGroup.WallLayer),
                (int)(door ? CollisionGroup.TableMask : CollisionGroup.FullTileMask));
    }

    private void OnBuild(Entity<XenomorphResinWhispererComponent> ent, ref XenomorphRemoteResinActionEvent args)
    {
        if (args.Handled || Transform(ent).GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid)) return;
        var tile = _map.TileIndicesFor(gridUid, grid, args.Target);
        var coordinates = new EntityCoordinates(gridUid, new Vector2(tile.X + .5f, tile.Y + .5f));
        if (!CanBuild(ent, coordinates, args.Door, out var remote)) return;
        _design.Modifiers(ent, coordinates, out var speed, out _);
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, (remote ? ent.Comp.RemoteDelayMultiplier : 1) * speed,
            new XenomorphRemoteResinDoAfterEvent { Coordinates = GetNetCoordinates(coordinates), Door = args.Door }, ent)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true, BlockDuplicate = true, CancelDuplicate = true });
    }

    private void OnBuilt(Entity<XenomorphResinWhispererComponent> ent, ref XenomorphRemoteResinDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !CanBuild(ent, GetCoordinates(args.Coordinates), args.Door, out _)
            || !_plasma.ChangePlasmaAmount(ent, -Cost(ent, GetCoordinates(args.Coordinates), args.Door))) return;
        Spawn(args.Door ? "ResinDoor" : "WallResin", GetCoordinates(args.Coordinates));
        args.Handled = true;
    }

    private void OnDoorVerb(Entity<XenomorphResinDoorComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<XenomorphResinWhispererComponent>(user, out var whisperer) || !CanOpen(user, ent, whisperer)) return;
        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("dumont-xenomorph-remote-resin-door"), Priority = 100,
            Act = () => { if (CanOpen(user, target, whisperer)) _doors.TryToggleDoor(target, user: user); },
        });
    }

    private bool CanOpen(EntityUid user, EntityUid target, XenomorphResinWhispererComponent whisperer)
        => Exists(user) && Exists(target) && _mobs.IsAlive(user) && OnWeeds(user)
            && _blocker.CanConsciouslyPerformAction(user) && HasComp<XenomorphResinDoorComponent>(target)
            && HasComp<DoorComponent>(target) && Transform(user).GridUid == Transform(target).GridUid
            && _interaction.InRangeUnobstructed(user, target, range: whisperer.Range);
}
