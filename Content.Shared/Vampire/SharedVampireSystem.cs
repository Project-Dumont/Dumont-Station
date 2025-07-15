// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <arguemeu@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Vampire.Components;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Vampire;

public sealed class SharedVampireSystem : EntitySystem
{
    public FixedPoint2 GetBloodEssence(EntityUid vampire)
    {
        if (!TryComp<VampireComponent>(vampire, out var comp))
            return 0;

        if (comp.Balance != null && comp.Balance.TryGetValue(VampireComponent.CurrencyProto, out var val))
            return val;

        return 0;
    }

    public void SetAlertBloodAmount(VampireAlertComponent component, int amount)
    {
        component.BloodAmount = amount;
        Dirty(component.Owner, component);
    }
}
