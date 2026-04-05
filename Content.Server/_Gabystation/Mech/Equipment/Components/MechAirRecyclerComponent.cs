// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Server._Gabystation.Mech.Equipment.EntitySystems;

namespace Content.Server._Gabystation.Mech.Equipment.Components;

[RegisterComponent, Access(typeof(MechAirRecyclerSystem))]
public sealed partial class MechAirRecyclerComponent : Component
{
    [DataField]
    public bool Enabled = false;

    [DataField]
    public float EnergyCost = 0.5f;

    [DataField]
    public float TargetPressure = Atmospherics.OneAtmosphere;

    [DataField]
    public float TargetTemperature = Atmospherics.T20C;

    [DataField]
    public float OxygenRatio = Atmospherics.OxygenStandard;

    [DataField]
    public float NitrogenRatio = Atmospherics.NitrogenStandard;
}
