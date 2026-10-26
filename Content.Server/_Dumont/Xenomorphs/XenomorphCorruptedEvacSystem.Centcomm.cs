using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Server.Shuttles.Components;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphCorruptedEvacSystem
{
    [Dependency] private MapLoaderSystem _loader = default!;

    private EntityUid? _corruptedCentcommMap;
    private EntityUid? _corruptedCentcommGrid;
    private readonly Dictionary<EntityUid, (EntityUid? Map, EntityUid? Grid)> _originalCentcomms = new();

    private void PrepareCorruptedCentcomm()
    {
        var stations = new List<Entity<StationCentcommComponent>>();
        var query = AllEntityQuery<StationCentcommComponent>();
        while (query.MoveNext(out var uid, out var comp))
            stations.Add((uid, comp));
        if (stations.Count == 0)
            return;

        if (!Exists(_corruptedCentcommMap) || !Exists(_corruptedCentcommGrid))
        {
            if (!_loader.TryLoadMap(new ResPath("/Maps/_Dumont/Centcomms/centcommcorrupted.yml"),
                    out var map, out var grids, new DeserializationOptions { InitializeMaps = true }))
            {
                Log.Error("Could not load the corrupted Centcomm for the xenomorph evacuation.");
                return;
            }

            if (grids.Count != 1)
            {
                Log.Error("The corrupted Centcomm must contain exactly one grid.");
                QueueDel(map.Value.Owner);
                return;
            }

            _corruptedCentcommMap = map.Value.Owner;
            _corruptedCentcommGrid = grids.Single().Owner;
            _map.SetPaused(map.Value.Owner, false);
        }

        foreach (var station in stations)
        {
            if (station.Comp.MapEntity == _corruptedCentcommMap)
                continue;
            _originalCentcomms.TryAdd(station.Owner, (station.Comp.MapEntity, station.Comp.Entity));
            station.Comp.MapEntity = _corruptedCentcommMap;
            station.Comp.Entity = _corruptedCentcommGrid;
        }
    }

    private void ResetCorruptedCentcomm()
    {
        foreach (var (station, original) in _originalCentcomms)
        {
            if (!TryComp<StationCentcommComponent>(station, out var centcomm)
                || centcomm.MapEntity != _corruptedCentcommMap)
                continue;
            centcomm.MapEntity = original.Map;
            centcomm.Entity = original.Grid;
        }
        _originalCentcomms.Clear();
        if (_corruptedCentcommMap is { } map && !TerminatingOrDeleted(map))
            QueueDel(map);
        _corruptedCentcommMap = null;
        _corruptedCentcommGrid = null;
    }
}
