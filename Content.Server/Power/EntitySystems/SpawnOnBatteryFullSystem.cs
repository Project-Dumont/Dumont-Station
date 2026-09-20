// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;

namespace Content.Server.Power.EntitySystems;

public sealed class SpawnOnBatteryFullSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpawnOnBatteryFullComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var comp, out var battery))
        {
            if (battery.CurrentCharge < battery.MaxCharge)
                continue;

            if (comp.Proto == null)
                SpawnFromEntityTable(uid, comp.Table);
            else
                Spawn(comp.Proto.Value, Transform(uid).Coordinates);

            _battery.SetCharge(uid, 0);
        }
    }

    private void SpawnFromEntityTable(EntityUid uid, EntityTableSelector? table)
    {
        foreach (var spawn in _entityTable.GetSpawns(table))
        {
            Spawn(spawn, Transform(uid).Coordinates);
        }
    }
}
