// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mech.Components;
using Content.Server._Gabystation.Mech.Equipment.Components;
using Content.Server.Mech.Systems;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Mech;
using Content.Shared.Atmos;
using Content.Shared.Mech.Components;

namespace Content.Server._Gabystation.Mech.Equipment.EntitySystems;

public sealed class MechAirRecyclerSystem : EntitySystem
{
    [Dependency] private readonly MechSystem _mech = default!;

    private float _timer;
    private const float UpdateInterval = 1.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechAirRecyclerComponent, InsertEquipmentEvent>(OnInsert);
        SubscribeLocalEvent<MechAirRecyclerComponent, MechEquipmentRemovedEvent>(OnRemove);
    }

    private void OnInsert(EntityUid uid, MechAirRecyclerComponent comp, InsertEquipmentEvent args)
    {
        comp.Enabled = true;
    }

    private void OnRemove(EntityUid uid, MechAirRecyclerComponent comp, MechEquipmentRemovedEvent args)
    {
        comp.Enabled = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _timer += frameTime;
        if (_timer < UpdateInterval)
            return;

        _timer -= UpdateInterval;

        var query = EntityQueryEnumerator<MechAirRecyclerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Enabled)
                continue;

            ProcessRecycler((uid, comp));
        }
    }

    private void ProcessRecycler(Entity<MechAirRecyclerComponent> recyclerEnt)
    {
        var (uid, recycler) = recyclerEnt;

        var xform = Transform(uid);
        var mechUid = xform.ParentUid;

        if (!TryComp<MechComponent>(mechUid, out var mech)
            || !TryComp<MechAirComponent>(mechUid, out var mechAir))
            return;

        var energyCost = recycler.EnergyCost * UpdateInterval;
        if (mech.Energy < energyCost)
            return;

        var air = mechAir.Air;

        // PV = nRT <=> n = PV/RT
        var targetMoles = (recycler.TargetPressure * air.Volume) / (Atmospherics.R * recycler.TargetTemperature);
        var currentMoles = air.TotalMoles;

        bool didWork = false;

        if (currentMoles < targetMoles)
        {
            var diff = targetMoles - currentMoles;
            air.AdjustMoles(Gas.Oxygen, diff * recycler.OxygenRatio);
            air.AdjustMoles(Gas.Nitrogen, diff * recycler.NitrogenRatio);
            didWork = true;
        }

        if (MathF.Abs(air.Temperature - recycler.TargetTemperature) > 0.5f)
        {
            air.Temperature = recycler.TargetTemperature;
            didWork = true;
        }

        if (didWork)
        {
            _mech.TryChangeEnergy(mechUid, -energyCost, mech);
        }
    }
}
