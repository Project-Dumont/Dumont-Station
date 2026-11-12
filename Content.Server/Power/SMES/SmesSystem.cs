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
public sealed partial class SmesSystem : EntitySystem // goob edit - made public
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(PowerNetSystem));

        SubscribeLocalEvent<SmesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SmesComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
        SubscribeLocalEvent<SmesComponent, PlayerSpawnCompleteEvent>(OnNewPlayerSpawn);             //Dumontstation change
        SubscribeLocalEvent<SmesComponent, CryoSlotReturnedEvent>(OnSlotReturned);                  //Dumontstation change
        _cfg.OnValueChanged(DumontCVars.SmesAutoRecharge, (value) => OnCvarChanged(value), true);   //Dumontstation change
    }

    private void OnMapInit(EntityUid uid, SmesComponent component, MapInitEvent args)
    {
        UpdateSmesState(uid, component);

        //Dumontstation Change
        if (!_cfg.GetCVar(DumontCVars.SmesAutoRecharge))
        {
            ToggleSelfRecharge(false);
        }
    }

    private void OnBatteryChargeChanged(EntityUid uid, SmesComponent component, ref ChargeChangedEvent args)
    {
        UpdateSmesState(uid, component);
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
