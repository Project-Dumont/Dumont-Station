// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

/// <summary>
/// Whether this entity has orbiting blades
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ProtectiveBladesComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Blades = new();

    [DataField]
    public float ProjectileSpeed = 6.5f;

    [DataField]
    public EntProtoId BlockShootStatus = "BlockProtectiveBladeShootStatusEffect";

    [DataField]
    public TimeSpan BladeShootDelay = TimeSpan.FromMilliseconds(250);
}
