// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Curses.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CurseOfFlamesStatusEffectComponent : Component
{
    [DataField]
    public float MinFireStacks = 2f;

    [DataField]
    public float Penetration = 0.5f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextIgnition = TimeSpan.Zero;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10);
}
