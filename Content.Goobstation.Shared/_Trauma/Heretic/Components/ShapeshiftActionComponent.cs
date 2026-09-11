// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Polymorph;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShapeshiftActionComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<PolymorphPrototype>> Polymorphs = new();

    [DataField]
    public LocId Speech = "heretic-speech-shapeshft";
}
