// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Ash;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CrackedLanternComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Summoned;

    [DataField]
    public EntProtoId<CrackedLanternSummonComponent> SummonProto = "LanternHereticSummon";
}
