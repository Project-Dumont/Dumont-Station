// SPDX-FileCopyrightText: 2023 Josh Bothun <joshbothun@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: MIT

using Content.Server._Starlight.Bed.Cryostorage;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared._Dumont.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Power;
using Content.Shared.Roles;
using Content.Shared.Rounding;
using Content.Shared.SMES;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Power.SMES;

public sealed partial class SmesSystem : EntitySystem // dumontstation, self recharge when there are no engineers
{
    [Dependency] private StationJobsSystem _stationJobsSystem = default!;
    [Dependency] private StationSystem _stationSystem = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private bool _selfRechargeEnabled = true;

    private void OnNewPlayerSpawn(EntityUid uid, SmesComponent smes, PlayerSpawnCompleteEvent args)
    {
        if (_selfRechargeEnabled && args.JobId is not null)
        {
            bool isEngineer = false;

            var engDept = _protoMan.Index<DepartmentPrototype>("Engineering");
            foreach (var role in engDept.Roles)
            {
                if (role == args.JobId)
                    isEngineer = true;
            }

            if (isEngineer)
            {
                ToggleSelfRecharge(false);
            }
        }
    }

    private void OnSlotReturned(EntityUid uid, SmesComponent smes, CryoSlotReturnedEvent args)
    {
        if (!_selfRechargeEnabled && _cfg.GetCVar(DumontCVars.SmesAutoRecharge))
        {
            if (GetEngineerCount() == 0)
                ToggleSelfRecharge(true);
        }
    }

    private void OnCvarChanged(bool value)
    {
        if (!value)
        {
            ToggleSelfRecharge(value);
        }
        else if (GetEngineerCount() == 0)
        {
            ToggleSelfRecharge(value);
        }
    }

    private void ToggleSelfRecharge(bool value)
    {
        if (_selfRechargeEnabled == value)
            return;

        _selfRechargeEnabled = value;
        var query = EntityQuery<SmesComponent, BatterySelfRechargerComponent>();
        foreach (var (smes, selfRecharger) in query)
        {
            selfRecharger.AutoRecharge = value;
        }
    }

    private int GetEngineerCount()
    {
        var stations = _stationSystem.GetStations();
        int totalEngCount = 0;
        foreach (var station in stations)
        {
            var jobs = _stationJobsSystem.GetJobs(station);
            var engDept = _protoMan.Index<DepartmentPrototype>("Engineering");

            foreach (var role in engDept.Roles)
            {
                jobs.TryGetValue(role, out var c);
                if (c.HasValue)
                    totalEngCount += c.Value;
            }
        }

        return totalEngCount;
    }
}
