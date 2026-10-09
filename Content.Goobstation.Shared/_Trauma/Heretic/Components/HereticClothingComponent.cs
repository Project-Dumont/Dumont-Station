// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class HereticClothingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public float DamagePopupProb = 0.25f;

    [DataField]
    public DamageSpecifier DamageOverTime = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2f },
            { "Slash", 2f },
            { "Piercing", 2f },
        }
    };
}
