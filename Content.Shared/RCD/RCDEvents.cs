// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RCD;

[Serializable, NetSerializable]
public sealed class RCDSystemMessage(ProtoId<RCDPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<RCDPrototype> ProtoId = protoId;
}


// Funky - Added to handle RPD layer selection
// This is a workaround for the fact eye rotation is not currently networked. Not intended as a permanent solution.
[Serializable, NetSerializable]
public sealed class RPDEyeRotationEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public float? EyeRotation;

    public RPDEyeRotationEvent(NetEntity netEntity, float? eyeRotation)
    {
        NetEntity = netEntity;
        EyeRotation = eyeRotation;
    }
}

[Serializable, NetSerializable]
public sealed class RCDConstructionGhostRotationEvent(NetEntity netEntity, Direction direction) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly Direction Direction = direction;
}

[Serializable, NetSerializable]
public sealed class RCDConstructionGhostFlipEvent(NetEntity netEntity, bool useMirrorPrototype) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly bool UseMirrorPrototype = useMirrorPrototype;
}

[Serializable, NetSerializable]
public enum RcdUiKey : byte
{
    Key
}
