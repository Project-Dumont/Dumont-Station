// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Ghoul;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FleshHereticMindComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Ghouls = new();

    [DataField, AutoNetworkedField]
    public int GhoulLimit = 3;

    [DataField, AutoNetworkedField]
    public DamageSpecifier WormSustainedDamage = new();
}

[Serializable, NetSerializable]
public enum HereticGhoulRecallKey : byte
{
    Key
}
