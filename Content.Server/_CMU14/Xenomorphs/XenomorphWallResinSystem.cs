using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphWallResinSystem : EntitySystem
{
    [Dependency] private MapSystem _map = default!;
    [Dependency] private TagSystem _tags = default!;

    private static readonly ProtoId<TagPrototype> WallTag = "Wall";
    private static readonly EntProtoId WallWeeds = "DumontXenomorphWallWeeds";
    private static readonly Vector2[] Offsets = [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY];

    public override void Initialize()
    {
        SubscribeLocalEvent<TagComponent, MapInitEvent>(OnWallInit);
        SubscribeLocalEvent<TagComponent, AnchorStateChangedEvent>(OnTaggedAnchor);
        SubscribeLocalEvent<XenomorphWeedWallSpreadComponent, MapInitEvent>(OnWeedInit);
        SubscribeLocalEvent<XenomorphWeedWallSpreadComponent, AnchorStateChangedEvent>(OnWeedAnchor);
        SubscribeLocalEvent<XenomorphWeedWallSpreadComponent, ComponentShutdown>(OnWeedRemoved);
        SubscribeLocalEvent<XenomorphWallResinComponent, EntityTerminatingEvent>(OnWallRemoved);
        SubscribeLocalEvent<XenomorphWallResinComponent, AnchorStateChangedEvent>(OnWallAnchor);
    }

    private void OnWallInit(Entity<TagComponent> ent, ref MapInitEvent args) => SpreadToWall(ent);

    private void OnTaggedAnchor(Entity<TagComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            SpreadToWall(ent);
    }

    private void SpreadToWall(EntityUid wall)
    {
        var xform = Transform(wall);
        if (!_tags.HasTag(wall, WallTag) || !xform.Anchored
            || xform.GridUid is not { } grid || !TryComp<MapGridComponent>(grid, out var mapGrid))
            return;

        foreach (var offset in Offsets)
        {
            foreach (var weed in _map.GetAnchoredEntities((grid, mapGrid), xform.Coordinates.Offset(offset)))
            {
                if (!TryComp<XenomorphWeedWallSpreadComponent>(weed, out var spread))
                    continue;

                var resin = EnsureComp<XenomorphWallResinComponent>(wall);
                resin.Sources.Add(weed);
                spread.Walls.Add(wall);
                if (!Exists(resin.Overlay))
                    resin.Overlay = Spawn(WallWeeds, new EntityCoordinates(wall, Vector2.Zero));
            }
        }
    }

    private void OnWeedAnchor(Entity<XenomorphWeedWallSpreadComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            SpreadFromWeed(ent);
    }

    private void OnWeedInit(Entity<XenomorphWeedWallSpreadComponent> ent, ref MapInitEvent args) => SpreadFromWeed(ent);

    private void SpreadFromWeed(Entity<XenomorphWeedWallSpreadComponent> ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not { } grid || !TryComp<MapGridComponent>(grid, out var mapGrid))
            return;

        foreach (var offset in Offsets)
        {
            foreach (var wall in _map.GetAnchoredEntities((grid, mapGrid), xform.Coordinates.Offset(offset)))
            {
                if (!_tags.HasTag(wall, WallTag))
                    continue;

                var resin = EnsureComp<XenomorphWallResinComponent>(wall);
                resin.Sources.Add(ent);
                ent.Comp.Walls.Add(wall);
                if (!Exists(resin.Overlay))
                    resin.Overlay = Spawn(WallWeeds, new EntityCoordinates(wall, Vector2.Zero));
            }
        }
    }

    private void OnWeedRemoved(Entity<XenomorphWeedWallSpreadComponent> ent, ref ComponentShutdown args)
    {
        foreach (var wall in ent.Comp.Walls)
        {
            if (!TryComp<XenomorphWallResinComponent>(wall, out var resin))
                continue;

            resin.Sources.Remove(ent);
            if (resin.Sources.Count == 0)
            {
                QueueDel(resin.Overlay);
                resin.Overlay = null;
            }
        }
    }

    private void ClearSources(Entity<XenomorphWallResinComponent> ent)
    {
        foreach (var source in ent.Comp.Sources)
        {
            if (TryComp<XenomorphWeedWallSpreadComponent>(source, out var spread))
                spread.Walls.Remove(ent.Owner);
        }
        ent.Comp.Sources.Clear();
    }

    private void OnWallRemoved(Entity<XenomorphWallResinComponent> ent, ref EntityTerminatingEvent args)
    {
        ClearSources(ent);
        QueueDel(ent.Comp.Overlay);
        QueueDel(ent.Comp.Nest);
    }

    private void OnWallAnchor(Entity<XenomorphWallResinComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            ClearSources(ent);
            QueueDel(ent.Comp.Overlay);
            ent.Comp.Overlay = null;
            QueueDel(ent.Comp.Nest);
            ent.Comp.Nest = null;
        }
    }
}
