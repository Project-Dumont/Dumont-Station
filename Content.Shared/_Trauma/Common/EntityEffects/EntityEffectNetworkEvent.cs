// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.EntityEffects;

/// <summary>
/// Events raised by RaiseNetworkEvents entity effect
/// Contains Entity that entity effect is applied to
/// </summary>
[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class EntityEffectNetworkEvent : EntityEventArgs
{
    [DataField]
    public NetEntity? Entity;
}
