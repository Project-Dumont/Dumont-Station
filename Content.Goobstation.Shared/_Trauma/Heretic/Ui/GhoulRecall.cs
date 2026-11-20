// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Ui;

[Serializable, NetSerializable]
public sealed class HereticGhoulRecallUiState(List<GhoulRecallData> ghouls) : BoundUserInterfaceState
{
    public List<GhoulRecallData> Ghouls = ghouls;
}

[Serializable, NetSerializable, DataRecord]
public partial record struct GhoulRecallData(NetEntity Ent, string Name, float? Distance);

[Serializable, NetSerializable]
public sealed class HereticGhoulRecallMessage(NetEntity ghoul) : BoundUserInterfaceMessage
{
    public NetEntity Ghoul = ghoul;
}
