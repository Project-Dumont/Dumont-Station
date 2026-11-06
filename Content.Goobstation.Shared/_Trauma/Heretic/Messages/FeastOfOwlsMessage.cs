// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Messages;

[Serializable, NetSerializable]
public sealed class FeastOfOwlsMessage(bool accepted) : BoundUserInterfaceMessage
{
    public readonly bool Accepted = accepted;
}

[Serializable, NetSerializable]
public enum FeastOfOwlsUiKey : byte
{
    Key
}
