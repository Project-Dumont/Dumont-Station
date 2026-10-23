// SPDX-FileCopyrightText: 2025 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Antag.MaintsSpawn;

public sealed class MaintsSpawnRule : StationEventSystem<MaintsSpawnRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MaintsSpawnRuleComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    // Dumont start
    private void OnSelectLocation(Entity<MaintsSpawnRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        var gameRule = Comp<GameRuleComponent>(args.GameRule);

        if (!TryGetRandomStation(out var station))
        {
            ForceEndSelf(ent.Owner, gameRule);
            return;
        }

        var locations = EntityQueryEnumerator<MaintsSpawnLocationComponent, TransformComponent>();
        var validLocations = new List<MapCoordinates>();
        while (locations.MoveNext(out _, out _, out var transform))
        {
            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station != station)
                continue;

            validLocations.Add(_transform.GetMapCoordinates(transform));
        }

        if (validLocations.Count == 0 && ent.Comp.FallbackToVents)
        {
            var vents = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
            while (vents.MoveNext(out _, out _, out var transform))
            {
                if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == station)
                    validLocations.Add(_transform.GetMapCoordinates(transform));
            }
        }

        if (validLocations.Count == 0)
        {
            ForceEndSelf(ent.Owner, gameRule);
            return;
        }

        args.Coordinates.AddRange(validLocations);
    }
    // Dumont end
}
