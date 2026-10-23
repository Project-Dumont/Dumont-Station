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

[UsedImplicitly]
public sealed class SmesSystem : EntitySystem // goob edit - made public
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly StationJobsSystem _stationJobsSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _selfRechargeEnabled = true;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(PowerNetSystem));

        SubscribeLocalEvent<SmesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SmesComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
        SubscribeLocalEvent<SmesComponent, PlayerSpawnCompleteEvent>(OnNewPlayerSpawn);
        SubscribeLocalEvent<SmesComponent, CryoSlotReturnedEvent>(OnSlotReturned);
        _cfg.OnValueChanged(DumontCVars.SmesAutoRecharge, (value) => OnCvarChanged(value), true);
    }

    private void OnMapInit(EntityUid uid, SmesComponent component, MapInitEvent args)
    {
        UpdateSmesState(uid, component);
        if (!_cfg.GetCVar(DumontCVars.SmesAutoRecharge))
        {
            ToggleSelfRecharge(false);
        }
    }

    private void OnBatteryChargeChanged(EntityUid uid, SmesComponent component, ref ChargeChangedEvent args)
    {
        UpdateSmesState(uid, component);
    }

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

    private void UpdateSmesState(EntityUid uid, SmesComponent smes)
    {
        var newLevel = CalcChargeLevel(uid);
        if (newLevel != smes.LastChargeLevel && smes.LastChargeLevelTime + smes.VisualsChangeDelay < _gameTiming.CurTime)
        {
            smes.LastChargeLevel = newLevel;
            smes.LastChargeLevelTime = _gameTiming.CurTime;

            _appearance.SetData(uid, SmesVisuals.LastChargeLevel, newLevel);
        }

        var newChargeState = CalcChargeState(uid);
        if (newChargeState != smes.LastChargeState && smes.LastChargeStateTime + smes.VisualsChangeDelay < _gameTiming.CurTime)
        {
            smes.LastChargeState = newChargeState;
            smes.LastChargeStateTime = _gameTiming.CurTime;

            _appearance.SetData(uid, SmesVisuals.LastChargeState, newChargeState);
        }
    }

    private int CalcChargeLevel(EntityUid uid, BatteryComponent? battery = null)
    {
        if (!Resolve(uid, ref battery, false))
            return 0;

        return ContentHelpers.RoundToLevels(battery.CurrentCharge, battery.MaxCharge, 6);
    }

    private ChargeState CalcChargeState(EntityUid uid, PowerNetworkBatteryComponent? netBattery = null)
    {
        if (!Resolve(uid, ref netBattery, false))
            return ChargeState.Still;

        return (netBattery.CurrentSupply - netBattery.CurrentReceiving) switch
        {
            > 0 => ChargeState.Discharging,
            < 0 => ChargeState.Charging,
            _ => ChargeState.Still
        };
    }
}
