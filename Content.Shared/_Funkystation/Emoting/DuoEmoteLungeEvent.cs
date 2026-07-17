// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Emoting;

/// <summary>
/// Raised on clients to trigger lunge animations for both participants
/// </summary>
[Serializable, NetSerializable]
public sealed class DuoEmoteLungeEvent(NetEntity initiator, NetEntity partner, string? animation) : EntityEventArgs
{
    public NetEntity Initiator { get; } = initiator;
    public NetEntity Partner { get; } = partner;
    public string? Animation { get; } = animation;
}
