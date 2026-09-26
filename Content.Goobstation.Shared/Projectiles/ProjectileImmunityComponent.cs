// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Goobstation.Shared.Projectiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileImmunityComponent : Component
{
    [DataField]
    public EntProtoId? DodgeEffect = "EffectParry";

    public HashSet<EntityUid> DodgedEntities = new();
}
