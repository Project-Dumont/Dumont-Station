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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

[RegisterComponent, NetworkedComponent]
public sealed partial class MansusInfusedComponent : Component
{
    [DataField]
    public int MaxCharges = 1;

    [DataField]
    public int AvailableCharges = 1;

    [DataField]
    public string HeldPrefix = "infused";

    [DataField]
    public ProtoId<EntityEffectPrototype> InfusedHitEffect = "HereticBladeGraspEffect";
}

[Serializable, NetSerializable]
public enum InfusedBladeVisuals
{
    Infused,
}
