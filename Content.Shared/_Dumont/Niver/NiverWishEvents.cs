// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Niver;

[Serializable, NetSerializable]
public sealed class NiverWishRequestEvent(TimeSpan duration) : EntityEventArgs
{
    public readonly TimeSpan Duration = duration;
}

[Serializable, NetSerializable]
public sealed class NiverWishResponseEvent(bool wished) : EntityEventArgs
{
    public readonly bool Wished = wished;
}
