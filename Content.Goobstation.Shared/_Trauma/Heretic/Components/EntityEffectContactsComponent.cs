// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityConditions;
using Content.Shared.EntityEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

/// <summary>
/// Applies entity effects to contacting entities every second
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EntityEffectContactsComponent : Component
{
    /// <summary>
    /// Used to prevent multiple of the same entity effects (same id) from being applied to colliding entity at once
    /// </summary>
    [DataField(required: true)]
    public string Id;

    [DataField(required: true)]
    public EntityEffect[] Effects;

    [DataField]
    public EntityCondition[]? Conditions;
}

[NetworkedComponent, RegisterComponent]
public sealed partial class EntityEffectContactsAffectedComponent : Component
{
    [DataField]
    public Dictionary<string, EntityUid> Contacts = new();
}
