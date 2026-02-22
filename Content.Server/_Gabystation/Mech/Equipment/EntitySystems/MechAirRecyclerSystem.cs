// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Components;
using Content.Server.Mech.Components;
using Content.Server._Gabystation.Mech.Equipment.Components;
using Content.Server.Mech.Systems;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Mech;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
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

            ProcessRecycler(uid);
        }
    }

    private void ProcessRecycler(EntityUid uid)
    {
        if (!TryComp<MechAirRecyclerComponent>(uid, out var recycler) || !recycler.Enabled)
            return;

        var xform = Transform(uid);
        var mechUid = xform.ParentUid;

        if (!TryComp<MechComponent>(mechUid, out var mech) ||
            !TryComp<MechAirComponent>(mechUid, out var mechAir))
            return;

        var energyCost = recycler.EnergyCost * UpdateInterval;
        if (mech.Energy < energyCost)
            return;

        var air = mechAir.Air;

        var targetMoles = (recycler.TargetPressure * air.Volume) / (Atmospherics.R * recycler.TargetTemperature);
        var currentMoles = air.TotalMoles;

        bool didwork = false;

        if (currentMoles < targetMoles)
        {
            var diff = targetMoles - currentMoles;
            air.AdjustMoles(Gas.Oxygen, diff * recycler.OxygenRatio);
            air.AdjustMoles(Gas.Nitrogen, diff * recycler.NitrogenRatio);
            didwork = true;
        }

        if (MathF.Abs(air.Temperature - recycler.TargetTemperature) > 0.5f)
        {
            air.Temperature = recycler.TargetTemperature;
            didwork = true;
        }

        if (didwork)
        {
            _mech.TryChangeEnergy(mechUid, -energyCost, mech);
        }
    }
}
