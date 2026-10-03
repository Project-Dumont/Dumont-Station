using Content.Shared._Starlight.Computers.PodConsole;
using Content.Shared.Buckle.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Dumont.EscapePods;

public sealed class EscapePodCapacitySystem : EntitySystem
{
    private EntityQuery<MobStateComponent> _mobQuery;
    private EntityQuery<StrapComponent> _strapQuery;

    private readonly HashSet<EntityUid> _occupants = new();

    public override void Initialize()
    {
        base.Initialize();

        _mobQuery = GetEntityQuery<MobStateComponent>();
        _strapQuery = GetEntityQuery<StrapComponent>();

        SubscribeLocalEvent<PodConsoleComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<PodConsoleComponent> ent, ref ExaminedEvent args)
    {
        if (GetMaxOccupants(ent) is not { } max)
            return;

        using (args.PushGroup(nameof(PodConsoleComponent)))
        {
            args.PushMarkup(Loc.GetString("escape-pod-max-occupants", ("max", max)));
        }
    }

    public int CountOccupants(EntityUid console)
    {
        if (Transform(console).GridUid is not { } grid)
            return 0;

        _occupants.Clear();
        var children = Transform(grid).ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            AddOccupants(child);
        }

        return _occupants.Count;
    }

    public int? GetMaxOccupants(Entity<PodConsoleComponent> console)
    {
        if (console.Comp.MaxOccupants is { } max)
            return max;

        if (Transform(console).GridUid is not { } grid)
            return null;

        var seats = 0;
        var children = Transform(grid).ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (_strapQuery.HasComp(child) && Transform(child).Anchored)
                seats++;
        }

        return seats > 0 ? seats : null;
    }

    public bool IsOvercrowded(Entity<PodConsoleComponent> console, out int occupants, out int max)
    {
        occupants = 0;
        max = 0;
        if (GetMaxOccupants(console) is not { } limit)
            return false;

        max = limit;
        occupants = CountOccupants(console);
        return occupants > max;
    }

    private void AddOccupants(EntityUid uid)
    {
        if (_mobQuery.HasComp(uid))
            _occupants.Add(uid);

        var children = Transform(uid).ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            AddOccupants(child);
        }
    }
}
