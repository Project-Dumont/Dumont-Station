// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Whitelist;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifierContactsComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string CollisionFixture;

    [DataField, AutoNetworkedField]
    public float Modifier = 1.0f;

    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? Blacklist;
}

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifiedByContactComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2? OriginalVelocity;
}
