using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server.Shuttles.Events;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Buckle;
using Content.Shared.Damage;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphCorruptedEvacComponent : Component;

public sealed partial class XenomorphCorruptedEvacSystem : EntitySystem
{
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private SharedBuckleSystem _buckle = default!;

    private bool _enabled;
    private static readonly Vector2[] Neighbours = [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY];

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<EmergencyShuttlePreDockEvent>(OnPreparing);
        InitializeTransmissions();
    }

    public void EnableCorruption()
    {
        _enabled = true;
        _emergency.TransitTime = 200f;
        PrepareCorruptedCentcomm();
    }

    private void OnRestart(RoundRestartCleanupEvent args)
    {
        _enabled = false;
        ResetTransmissions();
        ResetCorruptedCentcomm();
    }

    private void OnPreparing(ref EmergencyShuttlePreDockEvent args)
    {
        if (_enabled)
            PrepareCorruptedCentcomm();
        var shuttle = args.Shuttle;
        if (!_enabled || HasComp<XenomorphCorruptedEvacComponent>(shuttle)
            || !TryComp<MapGridComponent>(shuttle, out var grid))
            return;

        var floors = _map.GetAllTiles(shuttle, grid)
            .Where(tile => !_turf.IsSpace(tile) && !_turf.IsTileBlocked(tile, CollisionGroup.Impassable))
            .Select(tile => _map.GridTileToLocal(shuttle, grid, tile.GridIndices))
            .OrderBy(coords => coords.X).ThenBy(coords => coords.Y).ToList();
        if (floors.Count == 0)
            return;

        AddComp<XenomorphCorruptedEvacComponent>(shuttle);
        // Pick clear floor before adding the damageable resin, keeping furniture and doorways usable.
        var positions = floors.Where(coords => !_map.GetAnchoredEntities(shuttle, grid, coords)
            .Any(uid => HasComp<DamageableComponent>(uid))).ToList();

        foreach (var floor in floors)
        {
            if (!_map.GetAnchoredEntities(shuttle, grid, floor)
                .Any(uid => HasComp<XenomorphWeedWallSpreadComponent>(uid)))
                Spawn("ResinWeed", floor);
        }

        var chosen = new List<EntityCoordinates>();
        for (var i = 0; i < 3 && positions.Count > 0; i++)
        {
            var position = positions.OrderByDescending(coords => Clearance(coords))
                .ThenByDescending(coords => chosen.Count == 0 ? 0f : chosen.Min(other => Vector2.DistanceSquared(coords.Position, other.Position)))
                .First();
            positions.Remove(position);
            chosen.Add(position);
            var nest = Spawn("DumontXenomorphEvacNest", position);
            var corpse = Spawn("MobDumontXenomorphEvacERTCorpse", position);
            if (!_buckle.TryBuckle(corpse, null, nest, popup: false))
                Log.Warning($"Could not encase ERT corpse {ToPrettyString(corpse)} on corrupted evacuation shuttle.");
        }
        if (chosen.Count < 3)
            Log.Warning($"Corrupted evacuation shuttle {ToPrettyString(shuttle)} only had room for {chosen.Count} ERT corpses.");
    }

    private int Clearance(EntityCoordinates coords)
    {
        var clear = 0;
        foreach (var offset in Neighbours)
            if (_turf.GetTileRef(coords.Offset(offset)) is { } tile && !_turf.IsSpace(tile)
                && !_turf.IsTileBlocked(tile, CollisionGroup.MobMask))
                clear++;
        return clear;
    }
}
