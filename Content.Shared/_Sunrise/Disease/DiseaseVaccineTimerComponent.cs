// SPDX-FileCopyrightText: 2024 Lgibb18 <65973111+Lgibb18@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// © SUNRISE, An EULA/CLA with a hosting restriction, full text: https://github.com/space-sunrise/space-station-14/blob/master/CLA.txt
namespace Content.Shared._Sunrise.Disease;

[RegisterComponent]
public sealed partial class DiseaseVaccineTimerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ReadyAt = TimeSpan.Zero;

    [DataField] public TimeSpan Delay = TimeSpan.FromMinutes(5);

    [DataField] public float SpeedBefore = 0;
    [DataField] public bool Immune;
}
