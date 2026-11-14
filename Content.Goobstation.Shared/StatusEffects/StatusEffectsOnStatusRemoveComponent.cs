// SPDX-License-Identifier: AGPL-3.0-or-later


// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

namespace Content.Goobstation.Shared.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class StatusEffectsOnStatusRemoveComponent : Component
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, TimeSpan> StatusEffects;
}
