// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Polymorph;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Rituals;

[Serializable, NetSerializable]
public sealed class HereticRitualMessage(NetEntity ritual) : BoundUserInterfaceMessage
{
    public NetEntity Ritual = ritual;
}

[Serializable, NetSerializable]
public enum HereticRitualRuneUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class HereticShapeshiftMessage(ProtoId<PolymorphPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<PolymorphPrototype> ProtoId = protoId;
}


[Serializable, NetSerializable]
public enum HereticShapeshiftUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class EldritchIdMessage(EldritchIdConfiguration config) : BoundUserInterfaceMessage
{
    public EldritchIdConfiguration Config = config;
}

[Serializable, NetSerializable]
public enum EldritchIdUiKey : byte
{
    Key
}
