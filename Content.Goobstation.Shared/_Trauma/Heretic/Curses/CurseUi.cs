// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Curses;

[Serializable, NetSerializable, DataRecord]
public partial record struct CurseData(NetEntity Ent, string Name, float Multiplier, TimeSpan NextCurseTime);

[Serializable, NetSerializable]
public sealed class PickCurseVictimState(HashSet<CurseData> data) : BoundUserInterfaceState
{
    public HashSet<CurseData> Data = data;
}

[Serializable, NetSerializable]
public sealed class CurseSelectedMessage(NetEntity ent, EntProtoId curse) : BoundUserInterfaceMessage
{
    public NetEntity Victim = ent;

    public EntProtoId Curse = curse;
}

[Serializable, NetSerializable]
public enum HereticCurseUiKey : byte
{
    Key
}
