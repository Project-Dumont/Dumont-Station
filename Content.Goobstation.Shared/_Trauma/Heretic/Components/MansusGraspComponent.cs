// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MansusGraspComponent : Component
{
    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5f);

    [DataField]
    public float StaminaDamage = 40f;

    [DataField]
    public TimeSpan SpeechTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public TimeSpan AffectedTime = TimeSpan.FromMinutes(5);

    [DataField]
    public EntityEffect[]? Effects;
}
