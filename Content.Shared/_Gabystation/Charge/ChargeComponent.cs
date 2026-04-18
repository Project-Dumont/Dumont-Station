// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.Charge;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChargeComponent : Component
{
    [DataField]
    public TimeSpan TargetKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan WallKnockdown = TimeSpan.FromSeconds(2.5);

    [DataField]
    public bool StopOnFragileHit = true;

    [DataField]
    public bool KnockdownOnFragileHit;

    [DataField(required: true)]
    public EntProtoId ChargeAction;

    [DataField]
    public DamageSpecifier TargetDamage = new()
    {
        DamageDict =
        {
            { "Blunt", 8f }
        }
    };

    [DataField]
    public DamageSpecifier FragileDamage = new()
    {
        DamageDict =
        {
            { "Structural", 80f }
        }
    };

    [ViewVariables]
    public bool PendingCharge;

    [ViewVariables]
    public bool IsCharging;

    [ViewVariables]
    public HashSet<EntityUid> HitDuringCurrentCharge = new();
}
