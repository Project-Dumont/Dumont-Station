// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Server._Omu.Objectives.Components;
using Content.Shared._Omu.Clothing.Components;
using Content.Shared.Objectives.Components;
using Content.Server.Station.Systems;

namespace Content.Server._Omu.Objectives.Systems;

public sealed class CyberneticMantleExistsOnStationRequirementSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberneticMantleExistsOnStationRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, CyberneticMantleExistsOnStationRequirementComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        if (_station.GetOwningStation(args.Mind.OwnedEntity) is not { } station)
        {
            args.Cancelled = true;
            return;
        }

        // if there exists no item on station with a CyberneticMantleComponent
        var query = EntityQueryEnumerator<CyberneticMantleComponent>();
        while (query.MoveNext(out var mantle, out _))
        {
            if (_station.GetOwningStation(mantle) == station)
                return;
        }

        args.Cancelled = true;
    }
}
